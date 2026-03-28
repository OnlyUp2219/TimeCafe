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
} from "@fluentui/react-components";
import {
    Clock20Regular,
    DoorArrowRight20Regular,
    Money20Regular,
    Sticker20Regular,
} from "@fluentui/react-icons";
import {useEffect, useMemo, useState} from "react";
import {useAppSelector} from "@store/hooks";
import {useNavigate} from "react-router-dom";
import {BillingType as BillingTypeEnum, type BillingType} from "@app-types/tariff";
import {formatMoneyByN} from "@utility/formatMoney";
import {useProgressToast} from "@components/ToastProgress/ToastProgress";

import vortex from "@assets/vvvortex.svg";
import repeat from "@assets/rrrepeat (2).svg";
import surf from "@assets/sssurf.svg";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import "./visits.css";
import {useGetActiveVisitByUserQuery, useEndVisitMutation} from "@store/api/venueApi";
import {VisitEndDialog} from "./VisitEndDialog";
import {VisitDetailsCard} from "./VisitDetailsCard";
import {VisitAtmosphereCard} from "./VisitAtmosphereCard";

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
    const {showToast, ToasterElement} = useProgressToast();
    const userId = useAppSelector((state) => state.auth.userId);

    const {data: activeVisit, isLoading: loadingActiveVisit, error: visitError} = useGetActiveVisitByUserQuery(userId ?? "", {skip: !userId});
    const [endVisit, {isLoading: endingVisit}] = useEndVisitMutation();

    const startedAtMs = activeVisit ? Date.parse(activeVisit.entryTime) : 0;

    const [now, setNow] = useState(() => Date.now());
    const [confirmOpen, setConfirmOpen] = useState(false);
    const [exitComplete, setExitComplete] = useState(false);

    useEffect(() => {
        if (loadingActiveVisit || visitError) return;
        if (!activeVisit) {
            navigate("/visit/start", {replace: true});
        }
    }, [activeVisit, loadingActiveVisit, navigate, visitError]);

    useEffect(() => {
        const id = globalThis.setInterval(() => setNow(Date.now()), 1000);
        return () => globalThis.clearInterval(id);
    }, []);

    const elapsedSeconds = useMemo(() => {
        const ms = startedAtMs || now;
        const delta = now - ms;
        return Math.max(0, Math.floor(delta / 1000));
    }, [startedAtMs, now]);

    const elapsedMinutes = useMemo(() => Math.max(1, Math.ceil(elapsedSeconds / 60)), [elapsedSeconds]);

    const estimate = useMemo(() => {
        const billingType = activeVisit?.tariffBillingType ?? BillingTypeEnum.PerMinute;
        const pricePerMinute = activeVisit?.tariffPricePerMinute ?? 0;
        return calcEstimate(elapsedMinutes, billingType, pricePerMinute);
    }, [activeVisit?.tariffBillingType, activeVisit?.tariffPricePerMinute, elapsedMinutes]);

    const progressToNextHour = useMemo(() => {
        if (activeVisit?.tariffBillingType !== BillingTypeEnum.Hourly) return null;
        const minutesIntoHour = elapsedMinutes % 60;
        return minutesIntoHour / 60;
    }, [activeVisit?.tariffBillingType, elapsedMinutes]);

    const onFinishVisit = async () => {
        if (!activeVisit?.visitId) return;
        try {
            await endVisit(activeVisit.visitId).unwrap();
            setExitComplete(true);
            setConfirmOpen(false);
            showToast("Визит завершён", "success", "Готово");
            navigate("/visit/start", {replace: true});
        } catch {
            showToast("Не удалось завершить визит", "error", "Ошибка");
        }
    };

    if (loadingActiveVisit && !activeVisit) {
        return (
            <div className="tc-noise-overlay relative overflow-hidden min-h-full">
                {ToasterElement}
                <div className="mx-auto w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6 relative z-10">
                    <div className="rounded-3xl p-5 sm:p-8 tc-visits-panel" data-testid="visit-active-loading">
                        <Body1 block>Загружаем активный визит...</Body1>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="tc-noise-overlay relative overflow-hidden min-h-full">
            {ToasterElement}
            <div className="pointer-events-none absolute inset-0 overflow-hidden">
                <img
                    src={vortex}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[10vw] -left-[10vw] w-[55vw] max-w-[680px] select-none opacity-[0.22]"
                    draggable={false}
                />
                <img
                    src={repeat}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[6vw] -right-[8vw] w-[45vw] max-w-[560px] select-none opacity-[0.22]"
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[16vw] left-0 w-[100vw] max-w-none select-none opacity-[0.22]"
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[16vw] right-0 -scale-x-100 w-[100vw] max-w-none select-none opacity-[0.22]"
                    draggable={false}
                />
            </div>

            <div className="mx-auto w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6 relative z-10">
                <div className="rounded-3xl p-5 sm:p-8 tc-visits-panel" data-testid="visit-active-page">
                    <div className="flex flex-col gap-4">
                        <div className="flex flex-col gap-3">
                            <Title2 block>Вы в заведении</Title2>
                            <Body2 block>
                                Таймер и ориентировочная стоимость обновляются в реальном времени.
                            </Body2>
                        </div>

                        <Divider/>

                        <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                            <HoverTiltCard className="order-2 lg:order-1 lg:col-span-8">
                                <div className="flex flex-col gap-4">
                                    <div className="flex items-center justify-between gap-3 flex-wrap">
                                        <div className="flex items-center gap-2">
                                            <Clock20Regular/>
                                            <Subtitle2Stronger>Активный визит</Subtitle2Stronger>
                                        </div>
                                        <div className="flex items-center gap-2 flex-wrap">
                                            <Tag appearance="outline" icon={<Sticker20Regular/>}>
                                                {activeVisit?.tariffName ?? "Тариф"}
                                            </Tag>
                                            <Tag appearance="outline" data-testid="visit-active-estimate"
                                                 icon={<Money20Regular/>}>{formatMoneyByN(estimate.total)}</Tag>
                                        </div>
                                    </div>
                                    <Divider/>
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

                            <HoverTiltCard className="order-1 lg:order-2 lg:col-span-4">
                                <div className="flex flex-col gap-4">
                                    <div className="flex items-center gap-2">
                                        <DoorArrowRight20Regular/>
                                        <Subtitle2Stronger>Действия</Subtitle2Stronger>
                                    </div>
                                    <Divider/>
                                    <Button
                                        appearance="primary"
                                        icon={<DoorArrowRight20Regular/>}
                                        data-testid="visit-active-exit"
                                        onClick={() => setConfirmOpen(true)}
                                        disabled={exitComplete || endingVisit}
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
                    </div>
                </div>
            </div>
        </div>
    );
};
