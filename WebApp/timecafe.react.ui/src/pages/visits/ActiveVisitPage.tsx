import {
    Divider,
    Title2,
    Body1,
    Body2,
    Button,
    Card,
} from "@fluentui/react-components";
import { useEffect, useMemo, useState, useRef } from "react";
import { useAppDispatch, useAppSelector } from "@store/hooks";
import { useNavigate } from "react-router-dom";
import { BillingType as BillingTypeEnum } from "@app-types/tariff";
import { useProgressToast } from "@components/ToastProgress/ToastProgress";
import { useComponentSize } from "@hooks/useComponentSize";
import { calcVisitEstimate } from "@utility/visitEstimate";
import "./visits.css";
import { getRtkErrorMessage } from "@api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { venueApi, useGetActiveVisitByUserQuery, useRequestEndVisitMutation, useCancelVisitMutation, useGetAllPromotionsQuery, useGetUserLoyaltyQuery } from "@store/api/venueApi";
import { billingApi, useGetInvoiceByVisitIdQuery, usePayInvoiceMutation, useInitializeStripeInvoicePaymentMutation, useInitializeStripeCheckoutMutation, useGetBalanceQuery } from "@store/api/billingApi";
import { VisitStatus } from "@app-types/visit";
import { VisitEndDialog } from "./VisitEndDialog";
import { VisitCancelDialog } from "./VisitCancelDialog";
import { VisitStatusBadge } from "@components/VisitStatusBadge";
import { VirtualReceiptDialog } from "@components/VirtualReceipt/VirtualReceiptDialog";
import { profileApi, useGetProfileByUserIdQuery } from "@store/api/profileApi";

import { PendingVisitState } from "./states/PendingVisitState";
import { RejectedVisitState } from "./states/RejectedVisitState";
import { ApprovedVisitState } from "./states/ApprovedVisitState";
import { ActiveVisitState } from "./states/ActiveVisitState";
import { WaitingForPaymentState } from "./states/WaitingForPaymentState";
import { formatDurationSeconds } from "@utility/formatDurationSeconds";
import { useVisitDiscounts } from "@hooks/useVisitDiscounts";

export const ActiveVisitPage = () => {
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const { showToast, ToasterElement } = useProgressToast();
    const { sizes } = useComponentSize();
    const userId = useAppSelector((state) => state.auth.userId);

    const { data: activeVisit, isLoading: loadingActiveVisit, error: visitError, refetch } = useGetActiveVisitByUserQuery(
        userId ?? "",
        {
            skip: !userId
        }
    );
    const [requestEndVisit, { isLoading: endingVisit }] = useRequestEndVisitMutation();
    const [payInvoice, { isLoading: payingInvoice }] = usePayInvoiceMutation();
    const [initializeStripePayment, { isLoading: initializingStripe }] = useInitializeStripeInvoicePaymentMutation();
    const [initializeStripeCheckout] = useInitializeStripeCheckoutMutation();
    const [cancelVisit, { isLoading: cancellingVisit }] = useCancelVisitMutation();

    const { data: invoice, isLoading: loadingInvoice, refetch: refetchInvoice } = useGetInvoiceByVisitIdQuery(
        activeVisit?.visitId ?? "",
        {
            skip: !activeVisit || activeVisit.status !== VisitStatus.WaitingForPayment
        }
    );

    const { data: userBalance } = useGetBalanceQuery(userId ?? "", { skip: !userId });
    const { data: loyalty } = useGetUserLoyaltyQuery(userId ?? "", { skip: !userId });
    const { data: profile } = useGetProfileByUserIdQuery(userId ?? "", { skip: !userId });
    const { data: promotions } = useGetAllPromotionsQuery();

    const handlePayFromBalance = async () => {
        if (!invoice) return;
        try {
            await payInvoice({ invoiceId: invoice.invoiceId, method: 3 }).unwrap();
            showToast("Счёт успешно оплачен с баланса!", "success", "Успешно");
            void refetch();
        } catch (err) {
            const message = getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось оплатить счёт с баланса";
            showToast(message, "error", "Ошибка");
        }
    };

    const handlePayViaStripe = async () => {
        if (!invoice) return;
        try {
            const res = await initializeStripePayment({
                invoiceId: invoice.invoiceId,
                successUrl: globalThis.location.href,
                cancelUrl: globalThis.location.href
            }).unwrap();

            if (res.checkoutUrl) {
                globalThis.location.href = res.checkoutUrl;
            } else {
                showToast("Не удалось сгенерировать ссылку Stripe Checkout", "error", "Ошибка");
            }
        } catch (err) {
            const message = getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось инициализировать оплату Stripe";
            showToast(message, "error", "Ошибка");
        }
    };

    const startedAtMs = activeVisit ? Date.parse(activeVisit.entryTime) : 0;

    const [now, setNow] = useState(() => Date.now());
    const [confirmOpen, setConfirmOpen] = useState(false);
    const [cancelConfirmOpen, setCancelConfirmOpen] = useState(false);
    const [exitComplete, setExitComplete] = useState(false);
    const [receiptOpen, setReceiptOpen] = useState(false);

    const [gracePeriodEnd, setGracePeriodEnd] = useState<number | null>(null);
    const [graceSecondsLeft, setGraceSecondsLeft] = useState(0);

    const [graceAcknowledged, setGraceAcknowledged] = useState(false);
    const autoPayAttemptedRef = useRef<string | null>(null);

    useEffect(() => {
        if (activeVisit?.visitId) {
            const val = localStorage.getItem(`grace_acknowledged_${activeVisit.visitId}`);
            setGraceAcknowledged(val === "true");
        } else {
            setGraceAcknowledged(false);
        }
    }, [activeVisit?.visitId]);

    const handleSetGraceAcknowledged = (val: boolean) => {
        setGraceAcknowledged(val);
        if (activeVisit?.visitId) {
            localStorage.setItem(`grace_acknowledged_${activeVisit.visitId}`, val ? "true" : "false");
        }
    };

    useEffect(() => {
        if (activeVisit?.isFinishRequested && !gracePeriodEnd && !graceAcknowledged) {
            setGracePeriodEnd(Date.now() + 3 * 60 * 1000);
        } else if (!activeVisit?.isFinishRequested && gracePeriodEnd) {
            setGracePeriodEnd(null);
            handleSetGraceAcknowledged(false);
        }
    }, [activeVisit?.isFinishRequested, gracePeriodEnd, graceAcknowledged, activeVisit?.visitId]);

    useEffect(() => {
        if (!gracePeriodEnd) return;
        const interval = globalThis.setInterval(() => {
            const left = Math.max(0, Math.floor((gracePeriodEnd - Date.now()) / 1000));
            setGraceSecondsLeft(left);
        }, 1000);
        return () => globalThis.clearInterval(interval);
    }, [gracePeriodEnd]);

    const isActive = activeVisit?.status === VisitStatus.Active;

    useEffect(() => {
        if (loadingActiveVisit) return;
        let hasNoVisit = !activeVisit;
        if (activeVisit?.status === VisitStatus.Rejected) {
            const isRead = localStorage.getItem(`read_rejected_visit_${activeVisit.visitId}`) === "true";
            if (isRead) {
                hasNoVisit = true;
            }
        }
        if (visitError) {
            const is404 = (visitError as any).status === 404;
            if (is404) {
                hasNoVisit = true;
            }
        }
        if (hasNoVisit) {
            if (userId) {
                dispatch(venueApi.util.invalidateTags(["UserLoyalty", { type: "ActiveVisit", id: userId }]));
                dispatch(profileApi.util.invalidateTags([{ type: "Profile", id: userId }]));
                dispatch(billingApi.util.invalidateTags([{ type: "Balance", id: userId }, { type: "Debt", id: userId }]));
            }
            navigate("/visit/start", { replace: true });
        }
    }, [activeVisit, loadingActiveVisit, navigate, visitError, userId, dispatch]);

    useEffect(() => {
        if (!isActive) return;
        const id = globalThis.setInterval(() => setNow(Date.now()), 1000);
        return () => globalThis.clearInterval(id);
    }, [isActive]);

    useEffect(() => {
        if (activeVisit?.status === VisitStatus.Pending) {
            const timer = globalThis.setInterval(() => {
                void refetch();
            }, 3000);
            return () => globalThis.clearInterval(timer);
        }
    }, [activeVisit?.status, refetch]);

    useEffect(() => {
        if (activeVisit?.status === VisitStatus.WaitingForPayment && invoice && graceAcknowledged && userBalance) {
            const balanceAmount = userBalance.currentBalance ?? 0;
            const canPayWithBalance = balanceAmount >= invoice.totalAmount;
            if (canPayWithBalance && !payingInvoice && invoice.status === 1) {
                if (autoPayAttemptedRef.current !== invoice.invoiceId) {
                    autoPayAttemptedRef.current = invoice.invoiceId;
                    void handlePayFromBalance();
                }
            }
        }
    }, [activeVisit?.status, invoice, graceAcknowledged, userBalance, payingInvoice]);

    const elapsedSeconds = useMemo(() => {
        if (!startedAtMs) return 0;
        if (activeVisit?.status === VisitStatus.WaitingForPayment && activeVisit?.exitTime) {
            const delta = Date.parse(activeVisit.exitTime) - startedAtMs;
            return Math.max(0, Math.floor(delta / 1000));
        }
        if (!isActive) return 0;
        const ms = startedAtMs || now;
        const delta = now - ms;
        return Math.max(0, Math.floor(delta / 1000));
    }, [startedAtMs, now, isActive, activeVisit?.status, activeVisit?.exitTime]);

    const elapsedMinutes = useMemo(() => Math.max(1, Math.ceil(elapsedSeconds / 60)), [elapsedSeconds]);

    const { globalDiscount, tariffDiscount, personalDiscount } = useVisitDiscounts(
        promotions,
        loyalty,
        activeVisit?.tariffId
    );

    const estimate = useMemo(() => {
        const status = activeVisit?.status;
        const isCalculatable = status === VisitStatus.Active || status === VisitStatus.WaitingForPayment;
        if (!isCalculatable) return { total: 0, breakdown: "", baseTotal: 0, discountTotal: 0, appliedDiscountPercent: 0, isDiscounted: false };
        const billingType = activeVisit?.tariffBillingType ?? BillingTypeEnum.PerMinute;
        const pricePerMinute = activeVisit?.tariffPricePerMinute ?? 0;
        const minSessionMinutes = activeVisit?.tariffMinSessionMinutes ?? null;
        const roundingRule = activeVisit?.tariffRoundingRule ?? null;
        return calcVisitEstimate(elapsedMinutes, billingType, pricePerMinute, minSessionMinutes, roundingRule, globalDiscount, tariffDiscount, personalDiscount);
    }, [activeVisit?.tariffBillingType, activeVisit?.tariffPricePerMinute, activeVisit?.tariffMinSessionMinutes, activeVisit?.tariffRoundingRule, elapsedMinutes, activeVisit?.status, globalDiscount, tariffDiscount, personalDiscount]);

    const [payFromBalance, setPayFromBalance] = useState(false);

    const onFinishVisit = async (payFromBalanceFlag: boolean) => {
        if (!activeVisit?.visitId) return;
        setExitComplete(true);
        try {
            await requestEndVisit({ visitId: activeVisit.visitId, payFromBalance: payFromBalanceFlag }).unwrap();
            setConfirmOpen(false);
            setGracePeriodEnd(Date.now() + 3 * 60 * 1000);
            showToast("Запрос на выход успешно отправлен", "success", "Готово");
            void refetch();
        } catch (err) {
            setExitComplete(false);
            const message = getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось завершить визит";
            showToast(message, "error", "Ошибка");
        }
    };

    const handleGraceStripeCheckout = async () => {
        if (!userId) return;
        try {
            const res = await initializeStripeCheckout({
                userId,
                amount: estimate.total,
                successUrl: globalThis.location.href,
                cancelUrl: globalThis.location.href
            }).unwrap();

            if (res.checkoutUrl) {
                globalThis.location.href = res.checkoutUrl;
            } else {
                showToast("Не удалось сгенерировать ссылку Stripe Checkout", "error", "Ошибка");
            }
        } catch (err) {
            const message = getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось инициализировать оплату Stripe";
            showToast(message, "error", "Ошибка");
        }
    };

    const onCancelVisit = async () => {
        if (!activeVisit?.visitId) return;
        try {
            await cancelVisit(activeVisit.visitId).unwrap();
            setCancelConfirmOpen(false);
            showToast("Заявка отменена", "success", "Готово");
            navigate("/visit/start", { replace: true });
        } catch (err: any) {
            if (err?.status === 409) {
                setCancelConfirmOpen(false);
                showToast("Статус визита изменился. Обновляем данные...", "info", "Внимание");
                void refetch();
            } else {
                const message = getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось отменить заявку";
                showToast(message, "error", "Ошибка");
            }
        }
    };

    const handleGoToStart = () => {
        if (activeVisit?.status === VisitStatus.Rejected) {
            localStorage.setItem(`read_rejected_visit_${activeVisit.visitId}`, "true");
        }
        if (userId) {
            dispatch(venueApi.util.invalidateTags([{ type: "ActiveVisit", id: userId }]));
        }
        navigate("/visit/start", { replace: true });
    };

    if (loadingActiveVisit && !activeVisit) {
        return (
            <div className="relative min-h-full">
                {ToasterElement}
                <div className="page-content">
                    <Card size={sizes.card} data-testid="visit-active-loading">
                        <div className="flex">
                            <Body1>Загружаем визит...</Body1>
                        </div>
                    </Card>
                </div>
            </div>
        );
    }

    const renderContent = () => {
        if (!activeVisit) return null;

        if (activeVisit.status === VisitStatus.Pending) {
            return (
                <PendingVisitState
                    sizes={sizes}
                    cancellingVisit={cancellingVisit}
                    onCancelClick={() => setCancelConfirmOpen(true)}
                />
            );
        }

        if (activeVisit.status === VisitStatus.Approved) {
            return (
                <ApprovedVisitState
                    sizes={sizes}
                />
            );
        }

        if (activeVisit.status === VisitStatus.Rejected) {
            return (
                <RejectedVisitState
                    sizes={sizes}
                    rejectionReason={activeVisit.rejectionReason ?? undefined}
                    onGoToStart={handleGoToStart}
                />
            );
        }

        if (activeVisit.status === VisitStatus.Active) {
            return (
                <ActiveVisitState
                    activeVisit={activeVisit}
                    visitCount={profile?.visitCount ?? 0}
                    sizes={sizes}
                    estimate={estimate}
                    elapsedSeconds={elapsedSeconds}
                    now={now}
                    userBalance={userBalance}
                    personalDiscount={personalDiscount}
                    globalDiscount={globalDiscount}
                    tariffDiscount={tariffDiscount}
                    gracePeriodEnd={gracePeriodEnd}
                    graceSecondsLeft={graceSecondsLeft}
                    graceAcknowledged={graceAcknowledged}
                    exitComplete={exitComplete}
                    endingVisit={endingVisit}
                    onExitClick={() => setConfirmOpen(true)}
                    onGraceStripeCheckout={handleGraceStripeCheckout}
                    onAcknowledgeGrace={() => {
                        setGracePeriodEnd(null);
                        handleSetGraceAcknowledged(true);
                    }}
                    onChangePaymentMethod={() => handleSetGraceAcknowledged(false)}
                />
            );
        }

        if (activeVisit.status === VisitStatus.WaitingForPayment) {
            return (
                <WaitingForPaymentState
                    invoice={invoice}
                    loadingInvoice={loadingInvoice}
                    userBalance={userBalance}
                    sizes={sizes}
                    userId={userId}
                    payingInvoice={payingInvoice}
                    initializingStripe={initializingStripe}
                    personalDiscount={personalDiscount}
                    globalDiscount={globalDiscount}
                    tariffDiscount={tariffDiscount}
                    estimate={estimate}
                    onRetryInvoice={() => void refetchInvoice()}
                    onPayFromBalance={() => void handlePayFromBalance()}
                    onPayViaStripe={() => void handlePayViaStripe()}
                    onGoToStart={handleGoToStart}
                    onShowReceipt={() => setReceiptOpen(true)}
                />
            );
        }

        return (
            <div className="flex flex-col gap-4 items-center text-center py-12">
                <Body2>Неизвестный статус визита</Body2>
                <Button onClick={handleGoToStart}>Назад</Button>
            </div>
        );
    };

    let statusBadgeAndTag = null;
    let activeTariffName = "Тариф";

    if (activeVisit) {
        if (activeVisit.tariffName) {
            activeTariffName = activeVisit.tariffName;
        }

        statusBadgeAndTag = (
            <div className="flex items-center gap-2 flex-wrap">
                <VisitStatusBadge status={activeVisit.status} size="large" />
            </div>
        );
    }

    let receiptDialog = null;
    if (invoice && invoice.status === 2 && invoice.fiscalReceiptNumber) {
        receiptDialog = (
            <VirtualReceiptDialog
                open={receiptOpen}
                onOpenChange={setReceiptOpen}
                invoice={invoice}
                tariffName={activeTariffName}
            />
        );
    }

    return (
        <div className="relative min-h-full">
            {ToasterElement}
            <div className="page-content">
                <div data-testid="visit-active-page">
                    <div className="flex flex-col gap-4">
                        <div className="flex flex-col gap-3">
                            <div className="flex">
                                <Title2>Мой визит</Title2>
                            </div>
                            {statusBadgeAndTag}
                        </div>
                        <Divider />
                        {renderContent()}
                    </div>
                </div>
            </div>
            <VisitEndDialog
                open={confirmOpen}
                onOpenChange={setConfirmOpen}
                onConfirm={(payFromBalance) => void onFinishVisit(payFromBalance)}
                tariffName={activeTariffName}
                duration={formatDurationSeconds(elapsedSeconds)}
                estimateBaseTotal={estimate.baseTotal}
                estimateDiscountTotal={estimate.discountTotal}
                estimateAppliedDiscountPercent={estimate.appliedDiscountPercent}
                estimateIsDiscounted={estimate.isDiscounted}
                estimateTotal={estimate.total}
                estimateBreakdown={estimate.breakdown}
                personalDiscount={personalDiscount}
                globalDiscount={globalDiscount}
                tariffDiscount={tariffDiscount}
                confirming={endingVisit}
                userBalance={userBalance?.currentBalance ?? 0}
                payFromBalance={payFromBalance}
                onPayFromBalanceChange={setPayFromBalance}
            />
            <VisitCancelDialog
                open={cancelConfirmOpen}
                onOpenChange={setCancelConfirmOpen}
                onConfirm={() => void onCancelVisit()}
                visit={activeVisit ?? null}
                cancelling={cancellingVisit}
            />
            {receiptDialog}
        </div>
    );
};
