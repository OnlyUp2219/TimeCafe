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
} from "@fluentui/react-components";
import {
    Clock20Regular,
    DoorArrowRight20Regular,
    Money20Regular,
    Sticker20Regular,
    Dismiss20Regular,
    ArrowLeft20Regular,
} from "@fluentui/react-icons";
import { useEffect, useMemo, useState } from "react";
import { useAppSelector } from "@store/hooks";
import { useNavigate } from "react-router-dom";
import { BillingType as BillingTypeEnum, type BillingType } from "@app-types/tariff";
import { formatMoneyByN } from "@utility/formatMoney";
import { useProgressToast } from "@components/ToastProgress/ToastProgress";
import { useComponentSize } from "@hooks/useComponentSize";



import { HoverTiltCard } from "@components/HoverTiltCard/HoverTiltCard";
import "./visits.css";
import { getRtkErrorMessage } from "@api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useGetActiveVisitByUserQuery, useEndVisitMutation, useCancelVisitMutation } from "@store/api/venueApi";
import { VisitStatus } from "@app-types/visit";
import { VisitEndDialog } from "./VisitEndDialog";
import { VisitDetailsCard } from "./VisitDetailsCard";
import { VisitAtmosphereCard } from "./VisitAtmosphereCard";
import { VisitStatusBadge } from "@components/VisitStatusBadge";

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

type Estimate = {
    total: number;
    breakdown: string;
    chargedHours?: number;
    chargedMinutes?: number;
};

const calcEstimate = (elapsedMinutes: number, billingType: BillingType, pricePerMinute: number): Estimate => {
    const minutes = Math.max(1, Math.floor(elapsedMinutes));
    const safePricePerMinute = Math.max(0, pricePerMinute);

    if (billingType === BillingTypeEnum.PerMinute) {
        return {
            total: minutes * safePricePerMinute,
            chargedMinutes: minutes,
            breakdown: `${formatMoneyByN(safePricePerMinute)} / мин × ${minutes} мин`,
        };
    }

    const pricePerHour = safePricePerMinute * 60;
    const hours = Math.max(1, Math.ceil(minutes / 60));

    return {
        total: hours * pricePerHour,
        chargedHours: hours,
        breakdown: `${formatMoneyByN(pricePerHour)} / час × ${hours} ч`,
    };
};

export const ActiveVisitPage = () => {
    const navigate = useNavigate();
    const { showToast, ToasterElement } = useProgressToast();
    const { sizes } = useComponentSize();
    const userId = useAppSelector((state) => state.auth.userId);

    const { data: activeVisit, isLoading: loadingActiveVisit, error: visitError } = useGetActiveVisitByUserQuery(userId ?? "", { skip: !userId });
    const [endVisit, { isLoading: endingVisit }] = useEndVisitMutation();
    const [cancelVisit, { isLoading: cancellingVisit }] = useCancelVisitMutation();

    const startedAtMs = activeVisit ? Date.parse(activeVisit.entryTime) : 0;

    const [now, setNow] = useState(() => Date.now());
    const [confirmOpen, setConfirmOpen] = useState(false);
    const [exitComplete, setExitComplete] = useState(false);

    const isActive = activeVisit?.status === VisitStatus.Active;

    useEffect(() => {
        if (loadingActiveVisit || visitError) return;
        if (!activeVisit) {
            navigate("/visit/start", { replace: true });
        }
    }, [activeVisit, loadingActiveVisit, navigate, visitError]);

    useEffect(() => {
        if (!isActive) return;
        const id = globalThis.setInterval(() => setNow(Date.now()), 1000);
        return () => globalThis.clearInterval(id);
    }, [isActive]);

    const elapsedSeconds = useMemo(() => {
        if (!isActive) return 0;
        const ms = startedAtMs || now;
        const delta = now - ms;
        return Math.max(0, Math.floor(delta / 1000));
    }, [startedAtMs, now, isActive]);

    const elapsedMinutes = useMemo(() => Math.max(1, Math.ceil(elapsedSeconds / 60)), [elapsedSeconds]);

    const estimate = useMemo(() => {
        if (!isActive) return { total: 0, breakdown: "" };
        const billingType = activeVisit?.tariffBillingType ?? BillingTypeEnum.PerMinute;
        const pricePerMinute = activeVisit?.tariffPricePerMinute ?? 0;
        return calcEstimate(elapsedMinutes, billingType, pricePerMinute);
    }, [activeVisit?.tariffBillingType, activeVisit?.tariffPricePerMinute, elapsedMinutes, isActive]);

    const progressToNextHour = useMemo(() => {
        if (!isActive || activeVisit?.tariffBillingType !== BillingTypeEnum.Hourly) return null;
        const minutesIntoHour = elapsedMinutes % 60;
        return minutesIntoHour / 60;
    }, [activeVisit?.tariffBillingType, elapsedMinutes, isActive]);

    const onFinishVisit = async () => {
        if (!activeVisit?.visitId) return;
        try {
            await endVisit(activeVisit.visitId).unwrap();
            setExitComplete(true);
            setConfirmOpen(false);
            showToast("Визит завершён", "success", "Готово");
            navigate("/visit/start", { replace: true });
        } catch (err) {
            const message = getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось завершить визит";
            showToast(message, "error", "Ошибка");
        }
    };

    const onCancelVisit = async () => {
        if (!activeVisit?.visitId) return;
        try {
            await cancelVisit(activeVisit.visitId).unwrap();
            showToast("Заявка отменена", "success", "Готово");
            navigate("/visit/start", { replace: true });
        } catch (err) {
            const message = getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось отменить заявку";
            showToast(message, "error", "Ошибка");
        }
    };

    if (loadingActiveVisit && !activeVisit) {
        return (
            <div className="relative min-h-full">
                {ToasterElement}
                <div className="page-content">
                    <div className="rounded-3xl p-5 sm:p-8" data-testid="visit-active-loading">
                        <Body1 block>Загружаем визит...</Body1>
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
                <Button appearance="primary" size={sizes.button} onClick={() => navigate("/visit/start")} icon={<ArrowLeft20Regular />}>
                    К выбору тарифа
                </Button>
                <Button appearance="secondary" size={sizes.button} onClick={() => void onCancelVisit()} disabled={cancellingVisit} icon={<Dismiss20Regular />}>
                    {cancellingVisit ? "Отмена..." : "Отменить заявку"}
                </Button>
            </div>
        </Card>
    );

    const renderRejectedState = () => (
        <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center py-12">
            <Dismiss20Regular className="text-6xl opacity-50 text-[var(--colorPaletteRedForeground1)]" />
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
            <Clock20Regular className="text-6xl opacity-50 text-[var(--colorPaletteGreenForeground1)]" />
            <Title2>Визит подтверждён</Title2>
            <Body2>Можете входить в заведение!</Body2>
            <VisitStatusBadge status={VisitStatus.Approved} size="large" />
        </Card>
    );

    const renderActiveState = () => (
        <>
            <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                <HoverTiltCard className="order-2 lg:order-1 lg:col-span-8" size={sizes.card}>
                    <div className="flex flex-col gap-4">
                        <div className="flex items-center justify-between gap-3 flex-wrap">
                            <div className="flex items-center gap-2">
                                <Clock20Regular />
                                <Subtitle2Stronger>Активный визит</Subtitle2Stronger>
                            </div>
                            <div className="flex items-center gap-2 flex-wrap">
                                <Tag appearance="outline" icon={<Sticker20Regular />} size={sizes.badge}>
                                    {activeVisit?.tariffName ?? "Тариф"}
                                </Tag>
                                <Tag appearance="outline" data-testid="visit-active-estimate"
                                    icon={<Money20Regular />} size={sizes.badge}>{formatMoneyByN(estimate.total)}</Tag>
                            </div>
                        </div>
                        <Divider />
                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                            <div className="flex flex-col gap-2">
                                <Body1 block>Прошедшее время</Body1>
                                <LargeTitle block>{formatDuration(elapsedSeconds)}</LargeTitle>
                                <Body1 block>Обновление: каждую секунду</Body1>
                            </div>

                            <div className="flex flex-col gap-2 sm:items-end sm:text-right">
                                <Body1 block>Ориентировочная стоимость</Body1>
                                <Title1 block>{formatMoneyByN(estimate.total)}</Title1>
                                <Body1 block>{estimate.breakdown}</Body1>
                            </div>
                        </div>
                    </div>
                </HoverTiltCard>

                <HoverTiltCard className="order-1 lg:order-2 lg:col-span-4" size={sizes.card}>
                    <div className="flex flex-col gap-4">
                        <div className="flex items-center gap-2">
                            <DoorArrowRight20Regular />
                            <Subtitle2Stronger>Действия</Subtitle2Stronger>
                        </div>
                        <Divider />
                        <Button
                            appearance="primary"
                            icon={<DoorArrowRight20Regular />}
                            data-testid="visit-active-exit"
                            onClick={() => setConfirmOpen(true)}
                            disabled={exitComplete || endingVisit}
                            size={sizes.button}
                        >
                            Выход из заведения
                        </Button>

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
                    estimateTotal={estimate.total}
                    estimateBreakdown={estimate.breakdown}
                />

                <VisitAtmosphereCard
                    billingType={activeVisit?.tariffBillingType ?? BillingTypeEnum.PerMinute}
                    progressToNextHour={progressToNextHour}
                    elapsedMinutes={elapsedMinutes}
                />
            </div>

            <VisitEndDialog
                open={confirmOpen}
                onOpenChange={setConfirmOpen}
                onConfirm={() => void onFinishVisit()}
                tariffName={activeVisit?.tariffName ?? "Тариф"}
                duration={formatDuration(elapsedSeconds)}
                estimateTotal={estimate.total}
                estimateBreakdown={estimate.breakdown}
                confirming={endingVisit}
            />
        </>
    );

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
                <div className="rounded-3xl p-5 sm:p-8" data-testid="visit-active-page">
                    <div className="flex flex-col gap-4">
                        <div className="flex flex-col gap-3">
                            <Title2 block>Мой визит</Title2>
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
                </div>
            </div>
        </div>
    );
};
