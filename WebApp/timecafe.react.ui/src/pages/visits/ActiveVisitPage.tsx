import {
    Button,
    Divider,
    Tag,
    Title2,
    Title3,
    LargeTitle,
    Title1,
    Subtitle2Stronger,
    Body1,
    Body2,
    Card,
    MessageBar,
    MessageBarBody,
    MessageBarTitle,
    tokens,
} from "@fluentui/react-components";
import {
    Clock20Regular,
    DoorArrowRight20Regular,
    Money20Regular,
    Sticker20Regular,
    Dismiss20Regular,
    ArrowLeft20Regular,
    Payment20Regular,
} from "@fluentui/react-icons";
import { useEffect, useMemo, useState } from "react";
import { useAppSelector } from "@store/hooks";
import { useNavigate } from "react-router-dom";
import { BillingType as BillingTypeEnum } from "@app-types/tariff";
import { formatMoneyByN } from "@utility/formatMoney";
import { useProgressToast } from "@components/ToastProgress/ToastProgress";
import { useComponentSize } from "@hooks/useComponentSize";
import { calcVisitEstimate } from "@utility/visitEstimate";
import { HoverTiltCard } from "@components/HoverTiltCard/HoverTiltCard";
import "./visits.css";
import { getRtkErrorMessage } from "@api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useGetActiveVisitByUserQuery, useRequestEndVisitMutation, useCancelVisitMutation, useGetAllPromotionsQuery, useGetUserLoyaltyQuery } from "@store/api/venueApi";
import { useGetInvoiceByVisitIdQuery, usePayInvoiceMutation, useInitializeStripeInvoicePaymentMutation, useInitializeStripeCheckoutMutation, useGetBalanceQuery } from "@store/api/billingApi";
import { VisitStatus } from "@app-types/visit";
import { VisitEndDialog } from "./VisitEndDialog";
import { VisitCancelDialog } from "./VisitCancelDialog";
import { VisitDetailsCard } from "./VisitDetailsCard";
import { VisitAtmosphereCard } from "./VisitAtmosphereCard";
import { VisitStatusBadge } from "@components/VisitStatusBadge";
import { VirtualReceiptDialog } from "@components/VirtualReceipt/VirtualReceiptDialog";
import { Receipt20Regular } from "@fluentui/react-icons";

const pad2 = (value: number) => value.toString().padStart(2, "0");

const formatTimeHHmm = (hhmm: string, fallback: Date) => {
    if (/^\d{2}:\d{2}$/.test(hhmm)) return hhmm;
    return `${pad2(fallback.getHours())}:${pad2(fallback.getMinutes())}`;
};

const formatDuration = (totalSeconds: number) => {
    const safeSeconds = Math.max(0, Math.floor(totalSeconds));
    const hours = Math.floor(safeSeconds / 3600);
    const minutes = Math.floor((safeSeconds % 3600) / 60);
    const seconds = safeSeconds % 60;

    if (hours <= 0) return `${minutes}м ${pad2(seconds)}с`;
    return `${hours}ч ${pad2(minutes)}м ${pad2(seconds)}с`;
};

export const ActiveVisitPage = () => {
    const navigate = useNavigate();
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

    useEffect(() => {
        if (activeVisit?.isFinishRequested && !gracePeriodEnd && !graceAcknowledged) {
            setGracePeriodEnd(Date.now() + 3 * 60 * 1000);
        } else if (!activeVisit?.isFinishRequested && gracePeriodEnd) {
            setGracePeriodEnd(null);
            setGraceAcknowledged(false);
        }
    }, [activeVisit?.isFinishRequested, gracePeriodEnd, graceAcknowledged]);

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
        const hasNoVisit = !activeVisit || (visitError && (visitError as any).status === 404);
        if (hasNoVisit) {
            navigate("/visit/start", { replace: true });
        }
    }, [activeVisit, loadingActiveVisit, navigate, visitError]);

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

    const elapsedSeconds = useMemo(() => {
        if (!isActive) return 0;
        const ms = startedAtMs || now;
        const delta = now - ms;
        return Math.max(0, Math.floor(delta / 1000));
    }, [startedAtMs, now, isActive]);

    const elapsedMinutes = useMemo(() => Math.max(1, Math.ceil(elapsedSeconds / 60)), [elapsedSeconds]);

    const personalDiscount = loyalty?.personalDiscountPercent ?? 0;
    const nowStr = new Date().toISOString();
    const activePromotions = promotions?.filter((p: any) => p.isActive && p.validFrom <= nowStr && p.validTo >= nowStr) ?? [];
    const globalDiscount = activePromotions.length > 0
        ? Math.max(0, ...activePromotions.filter((p: any) => p.type === 0).map((p: any) => p.discountPercent ?? 0))
        : 0;
    const tariffDiscount = (activeVisit && activePromotions.length > 0)
        ? Math.max(0, ...activePromotions.filter((p: any) => p.type === 1 && p.tariffId === activeVisit.tariffId).map((p: any) => p.discountPercent ?? 0))
        : 0;

    const estimate = useMemo(() => {
        if (!isActive) return { total: 0, breakdown: "", baseTotal: 0, discountTotal: 0, appliedDiscountPercent: 0, isDiscounted: false };
        const billingType = activeVisit?.tariffBillingType ?? BillingTypeEnum.PerMinute;
        const pricePerMinute = activeVisit?.tariffPricePerMinute ?? 0;
        const minSessionMinutes = activeVisit?.tariffMinSessionMinutes ?? null;
        const roundingRule = activeVisit?.tariffRoundingRule ?? null;
        return calcVisitEstimate(elapsedMinutes, billingType, pricePerMinute, minSessionMinutes, roundingRule, globalDiscount, tariffDiscount, personalDiscount);
    }, [activeVisit?.tariffBillingType, activeVisit?.tariffPricePerMinute, activeVisit?.tariffMinSessionMinutes, activeVisit?.tariffRoundingRule, elapsedMinutes, isActive, globalDiscount, tariffDiscount, personalDiscount]);



    const onFinishVisit = async () => {
        if (!activeVisit?.visitId) return;
        setExitComplete(true);
        try {
            await requestEndVisit(activeVisit.visitId).unwrap();
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

    if (loadingActiveVisit && !activeVisit) {
        return (
            <div className="relative min-h-full">
                {ToasterElement}
                <div className="page-content">
                    <div className="rounded-3xl p-5 sm:p-8" data-testid="visit-active-loading">
                        <div className="flex">
                            <Body1>Загружаем визит...</Body1>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    const renderPendingState = () => (
        <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center py-12">
            <Clock20Regular className="text-6xl opacity-50" />
            <Title2>Ожидаем подтверждения</Title2>
            <Body2>Менеджер скоро рассмотрит вашу заявку на визит.</Body2>
            <div className="flex gap-2 flex-wrap justify-center">
                <Button appearance="primary" size={sizes.button} onClick={() => setCancelConfirmOpen(true)} disabled={cancellingVisit} icon={<Dismiss20Regular />}>
                    Отменить заявку
                </Button>
            </div>
        </Card>
    );

    const renderRejectedState = () => (
        <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center py-12">
            <Dismiss20Regular className="text-6xl opacity-50 text-(--colorPaletteRedForeground1)" />
            <Title2>Заявка отклонена</Title2>
            {activeVisit?.rejectionReason && (
                <Body2>Причина: {activeVisit.rejectionReason}</Body2>
            )}
            <Button appearance="primary" size={sizes.button} onClick={() => navigate("/visit/start")} icon={<ArrowLeft20Regular />}>
                К выбору тарифа
            </Button>
        </Card>
    );

    const renderApprovedState = () => (
        <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center py-12">
            <Clock20Regular className="text-6xl opacity-50 text-(--colorPaletteGreenForeground1)" />
            <Title2>Визит подтверждён</Title2>
            <Body2>Можете входить в заведение!</Body2>
            <VisitStatusBadge status={VisitStatus.Approved} size="large" />
        </Card>
    );

    const renderActiveState = () => (
        <>
            <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                {activeVisit?.isFinishRequested && (
                    <div className="lg:col-span-12">
                        <MessageBar intent="warning">
                            <MessageBarBody>
                                <MessageBarTitle>Запрос на завершение визита отправлен</MessageBarTitle>
                                Мы зафиксируем точное время окончания сессии, как только вы подойдете к стойке ресепшена. В целях безопасности ваш таймер активен до подтверждения администратором на кассе.
                            </MessageBarBody>
                        </MessageBar>
                    </div>
                )}

                <HoverTiltCard className="order-2 lg:order-1 lg:col-span-8 h-full flex flex-col" size={sizes.card}>
                    <div className="flex flex-col gap-4 h-full flex-1 justify-between">
                        <div className="flex flex-col gap-4 flex-1">
                            <div className="flex items-center justify-between gap-3 flex-wrap">
                                <div className="flex items-center gap-2">
                                    <Clock20Regular />
                                    <Subtitle2Stronger>Активный визит</Subtitle2Stronger>
                                </div>
                                <div className="flex items-center gap-2 flex-wrap">
                                    <Tag appearance="outline" icon={<Sticker20Regular />}>
                                        {activeVisit?.tariffName ?? "Тариф"}
                                    </Tag>
                                    <Tag appearance="outline" data-testid="visit-active-estimate"
                                        icon={<Money20Regular />} >{formatMoneyByN(estimate.total)}</Tag>
                                    {estimate.isDiscounted && (
                                        <Tag appearance="brand" color="success">
                                            -{estimate.appliedDiscountPercent}%
                                        </Tag>
                                    )}
                                </div>
                            </div>
                            <Divider />
                            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                                <div className="flex flex-col gap-2">
                                    <div className="flex">
                                        <Body1>Прошедшее время</Body1>
                                    </div>
                                    <div className="flex">
                                        <LargeTitle>{formatDuration(elapsedSeconds)}</LargeTitle>
                                    </div>
                                    <div className="flex">
                                        <Body1>Обновление: каждую секунду</Body1>
                                    </div>
                                </div>

                                <div className="flex flex-col gap-2 sm:items-end sm:text-right">
                                    <div className="flex justify-end">
                                        <Body1>Ориентировочная стоимость</Body1>
                                    </div>
                                    <div className="flex justify-end">
                                        <Title1>{formatMoneyByN(estimate.total)}</Title1>
                                    </div>
                                    <div className="flex justify-end">
                                        <Body1>{estimate.breakdown}</Body1>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </HoverTiltCard>

                <HoverTiltCard className="order-1 lg:order-2 lg:col-span-4 h-full flex flex-col" size={sizes.card}>
                    <div className="flex flex-col gap-4 h-full flex-1 justify-between">
                        <div className="flex items-center gap-2">
                            <DoorArrowRight20Regular />
                            <Subtitle2Stronger>Действия</Subtitle2Stronger>
                        </div>
                        <Divider />
                        {activeVisit?.isFinishRequested && gracePeriodEnd !== null && !graceAcknowledged ? (
                            <Card size={sizes.card} style={{ backgroundColor: tokens.colorStatusWarningBackground1, borderColor: tokens.colorStatusWarningBorder1 }}>
                                <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-2" style={{ color: tokens.colorStatusWarningForeground1 }}>
                                        <Clock20Regular />
                                        <Subtitle2Stronger>Ожидаем оплату</Subtitle2Stronger>
                                    </div>
                                    <Title1 style={{ color: tokens.colorStatusWarningForeground1 }}>
                                        {Math.floor(graceSecondsLeft / 60)}:{(graceSecondsLeft % 60).toString().padStart(2, '0')}
                                    </Title1>
                                </div>
                                <Body1>Для завершения визита без задержек пополните баланс картой через Stripe или оплатите визит наличными у администратора. Если льготное время истечет, таймер биллинга возобновит работу.</Body1>
                                <div className="flex flex-col gap-2">
                                    <Button
                                        appearance="primary"
                                        onClick={handleGraceStripeCheckout}
                                        icon={<Payment20Regular />}
                                    >
                                        Пополнить через Stripe
                                    </Button>
                                    <Button
                                        onClick={() => {
                                            setGracePeriodEnd(null);
                                            setGraceAcknowledged(true);
                                        }}
                                        icon={<Money20Regular />}
                                        disabled={userBalance?.currentBalance !== undefined && userBalance.currentBalance < estimate.total}
                                    >
                                        {userBalance?.currentBalance !== undefined && userBalance.currentBalance < estimate.total
                                            ? "Недостаточно средств на балансе"
                                            : "У меня хватает баланса"}
                                    </Button>
                                </div>
                            </Card>
                        ) : (
                            <Button
                                appearance="primary"
                                icon={<DoorArrowRight20Regular />}
                                data-testid="visit-active-exit"
                                onClick={() => setConfirmOpen(true)}
                                disabled={exitComplete || endingVisit || activeVisit?.isFinishRequested}
                                size={sizes.button}
                            >
                                {activeVisit?.isFinishRequested ? "Ожидание фиксации времени" : "Выход из заведения"}
                            </Button>
                        )}

                        <div className="flex flex-col gap-2">
                            <div className="flex items-center justify-between gap-3">
                                <Body1>Время входа</Body1>
                                <Title3>
                                    {formatTimeHHmm("", new Date(startedAtMs ?? now))}
                                </Title3>
                            </div>
                            <div className="flex items-center justify-between gap-3">
                                <Body1>Тип</Body1>
                                <Body1>
                                    {activeVisit?.tariffBillingType === BillingTypeEnum.Hourly ? "Почасовой" : "Поминутный"}
                                </Body1>
                            </div>
                        </div>
                    </div>
                </HoverTiltCard>
            </div>

            <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                <VisitDetailsCard
                    tariffName={activeVisit?.tariffName ?? "Тариф"}
                    billingType={activeVisit?.tariffBillingType ?? BillingTypeEnum.PerMinute}
                    estimate={estimate}
                    personalDiscount={personalDiscount}
                    globalDiscount={globalDiscount}
                    tariffDiscount={tariffDiscount}
                />

                <VisitAtmosphereCard
                    billingType={activeVisit?.tariffBillingType ?? BillingTypeEnum.PerMinute}
                    tariffPricePerMinute={activeVisit?.tariffPricePerMinute ?? 0}
                    minSessionMinutes={activeVisit?.tariffMinSessionMinutes ?? null}
                    roundingRule={activeVisit?.tariffRoundingRule ?? null}
                    guestsCount={activeVisit?.guestsCount ?? 1}
                />
            </div>
        </>
    );

    const renderWaitingForPaymentState = () => {
        if (loadingInvoice) {
            return (
                <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center py-12">
                    <Clock20Regular className="text-6xl opacity-50 animate-spin" />
                    <Title2>Загрузка счёта...</Title2>
                    <Body2>Пожалуйста, подождите, мы получаем информацию об инвойсе.</Body2>
                </Card>
            );
        }

        if (!invoice) {
            return (
                <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center py-12">
                    <Dismiss20Regular className="text-6xl opacity-50 text-(--colorPaletteRedForeground1)" />
                    <Title2>Счёт не найден</Title2>
                    <Body2>Не удалось загрузить информацию о счёте для оплаты визита.</Body2>
                    <Button onClick={() => void refetchInvoice()}>Попробовать снова</Button>
                </Card>
            );
        }

        const isPaid = invoice.status === 2; // Paid
        if (isPaid) {
            return (
                <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center py-12">
                    <Clock20Regular className="text-6xl text-(--colorPaletteGreenForeground1)" />
                    <Title2>Визит успешно оплачен!</Title2>
                    <Body2>Благодарим вас за визит! Ваш столик освобожден. Будем рады видеть вас снова!</Body2>
                    <div className="flex gap-2 flex-wrap justify-center">
                        <Button appearance="primary" size={sizes.button} onClick={() => navigate("/visit/start")}>
                            К выбору тарифа
                        </Button>
                        {invoice.fiscalReceiptNumber && (
                            <Button size={sizes.button} appearance="outline" icon={<Receipt20Regular />} onClick={() => setReceiptOpen(true)}>
                                Показать чек
                            </Button>
                        )}
                    </div>
                </Card>
            );
        }

        const balanceAmount = userBalance?.currentBalance ?? 0;
        const canPayWithBalance = balanceAmount >= invoice.totalAmount;

        return (
            <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                <HoverTiltCard className="lg:col-span-8" size={sizes.card}>
                    <div className="flex flex-col gap-4">
                        <div className="flex items-center gap-2">
                            <Money20Regular className="text-2xl" />
                            <Subtitle2Stronger>Счёт на оплату визита</Subtitle2Stronger>
                        </div>
                        <Divider />

                        <div className="flex flex-col gap-3">
                            <div className="flex justify-between items-center bg-(--colorNeutralBackground1) p-2">
                                <Body1>Итого к оплате:</Body1>
                                <LargeTitle className="text-(--colorBrandForeground1)">{formatMoneyByN(invoice.totalAmount)}</LargeTitle>
                            </div>

                            <div className="grid grid-cols-2 gap-2 text-sm p-2">
                                <span className="opacity-75">ID счёта:</span>
                                <span className="text-right font-mono text-xs">{invoice.invoiceId}</span>

                                <span className="opacity-75">Время создания:</span>
                                <span className="text-right">{new Date(invoice.createdAt).toLocaleString()}</span>
                            </div>
                        </div>

                        <Divider />
                        <div className="flex flex-col gap-3">
                            <Subtitle2Stronger>Способы оплаты</Subtitle2Stronger>

                            <div className={`grid grid-cols-1 ${userId ? 'md:grid-cols-2' : ''} gap-4`}>
                                {userId && (
                                    <Card size={sizes.card} className="flex flex-col gap-3 justify-between">
                                        <div>
                                            <Subtitle2Stronger className="block mb-1">С внутреннего баланса</Subtitle2Stronger>
                                            <Body2 className="block mb-2">Быстрая оплата в одно нажатие.</Body2>
                                            <div className="text-sm bg-(--colorNeutralBackground2) p-2 rounded-lg flex justify-between">
                                                <span>Ваш баланс:</span>
                                                <span className={canPayWithBalance ? "text-green-500 font-bold" : "text-red-500 font-bold"}>
                                                     {formatMoneyByN(balanceAmount)}
                                                </span>
                                            </div>
                                        </div>
                                        <Button
                                            appearance="primary"
                                            onClick={() => void handlePayFromBalance()}
                                            disabled={!canPayWithBalance || payingInvoice}
                                            icon={<Money20Regular />}
                                        >
                                            Оплатить с баланса
                                        </Button>
                                    </Card>
                                )}

                                <Card size={sizes.card} className="flex flex-col gap-3 justify-between">
                                    <div>
                                        <Subtitle2Stronger className="block mb-1">Банковской картой (Stripe)</Subtitle2Stronger>
                                        <Body2 className="block mb-2">Безопасная онлайн-оплата через Stripe Checkout.</Body2>
                                    </div>
                                    <Button
                                        appearance="primary"
                                        onClick={() => void handlePayViaStripe()}
                                        disabled={initializingStripe}
                                        icon={<DoorArrowRight20Regular />}
                                    >
                                        Оплатить картой онлайн
                                    </Button>
                                </Card>
                            </div>
                        </div>
                    </div>
                </HoverTiltCard>

                <HoverTiltCard className="lg:col-span-4" size={sizes.card}>
                    <div className="flex flex-col gap-4">
                        <div className="flex items-center gap-2">
                            <Clock20Regular />
                            <Subtitle2Stronger>Оплата на кассе</Subtitle2Stronger>
                        </div>
                        <Divider />
                        <Body2 className="block text-justify leading-relaxed">
                            Вы также можете подойти к стойке администратора и произвести оплату наличными средствами или через банковский терминал.
                        </Body2>
                        <MessageBar intent="warning">
                            <MessageBarBody>
                                После фиксации администратором оплаты на кассе, ваш визит автоматически перейдет в статус завершенного, а столик освободится.
                            </MessageBarBody>
                        </MessageBar>
                    </div>
                </HoverTiltCard>
            </div>
        );
    };

    const renderContent = () => {
        if (!activeVisit) return null;
        switch (activeVisit.status) {
            case VisitStatus.Pending:
                return renderPendingState();
            case VisitStatus.Approved:
                return renderApprovedState();
            case VisitStatus.Rejected:
                return renderRejectedState();
            case VisitStatus.Active:
                return renderActiveState();
            case VisitStatus.WaitingForPayment:
                return renderWaitingForPaymentState();
            default:
                return (
                    <div className="flex flex-col gap-4 items-center text-center py-12">
                        <Body2>Неизвестный статус визита</Body2>
                        <Button onClick={() => navigate("/visit/start")}>Назад</Button>
                    </div>
                );
        }
    };

    return (
        <div className="relative min-h-full">
            {ToasterElement}
            <div className="page-content">
                <Card size={sizes.card} data-testid="visit-active-page">
                    <div className="flex flex-col gap-4">
                        <div className="flex flex-col gap-3">
                            <div className="flex">
                                <Title2>Мой визит</Title2>
                            </div>
                            {activeVisit && (
                                <div className="flex items-center gap-2 flex-wrap">
                                    <VisitStatusBadge status={activeVisit.status} size="large" />
                                    <Tag appearance="outline" icon={<Sticker20Regular />}>
                                        {activeVisit.tariffName ?? "Тариф"}
                                    </Tag>
                                </div>
                            )}
                        </div>
                        <Divider />
                        {renderContent()}
                    </div>
                </Card>
            </div>
            <VisitEndDialog
                open={confirmOpen}
                onOpenChange={setConfirmOpen}
                onConfirm={() => void onFinishVisit()}
                tariffName={activeVisit?.tariffName ?? "Тариф"}
                duration={formatDuration(elapsedSeconds)}
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
            />
            <VisitCancelDialog
                open={cancelConfirmOpen}
                onOpenChange={setCancelConfirmOpen}
                onConfirm={() => void onCancelVisit()}
                visit={activeVisit ?? null}
                cancelling={cancellingVisit}
            />
            {invoice && invoice.status === 2 && invoice.fiscalReceiptNumber && (
                <VirtualReceiptDialog
                    open={receiptOpen}
                    onOpenChange={setReceiptOpen}
                    invoice={invoice}
                    tariffName={activeVisit?.tariffName ?? "Тариф"}
                />
            )}
        </div>
    );
};
