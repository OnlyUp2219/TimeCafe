import {
    Button,
    Card,
    Divider,
    Tag,
    Title2,
    Title3,
    LargeTitle,
    Title1,
    Subtitle2Stronger,
    Body1,
    Body2,
    Dialog,
    DialogBody,
    DialogSurface,
    DialogTitle,
    DialogActions,
    DialogContent,
    MessageBar,
    MessageBarActions,
    MessageBarBody,
    MessageBarTitle,
    ProgressBar,
} from "@fluentui/react-components";
import {
    Clock20Regular,
    DoorArrowRight20Regular,
    Money20Regular,
    Shield20Regular,
    Info20Regular,
    Sticker20Regular,
    WeatherMoon20Regular,
    ArrowClockwise20Regular,
} from "@fluentui/react-icons";
import {useEffect, useMemo, useState} from "react";
import {useDispatch, useSelector} from "react-redux";
import {useNavigate} from "react-router-dom";
import type {AppDispatch, RootState} from "@store";
import {clearVisitError, endVisitOnServer, loadActiveVisitByUser, VisitUiStatus} from "@store/visitSlice";
import {BillingType as BillingTypeEnum, type BillingType} from "@app-types/tariff";
import {formatMoneyByN} from "@utility/formatMoney";
import {useProgressToast} from "@components/ToastProgress/ToastProgress";

import vortex from "@assets/vvvortex.svg";
import repeat from "@assets/rrrepeat (2).svg";
import surf from "@assets/sssurf.svg";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import "./visits.css";

const clampMin = (value: number, min: number) => Math.max(min, value);

const pad2 = (value: number) => value.toString().padStart(2, "0");

const formatTimeHHmm = (hhmm: string, fallback: Date) => {
    if (/^\d{2}:\d{2}$/.test(hhmm)) return hhmm;
    return `${pad2(fallback.getHours())}:${pad2(fallback.getMinutes())}`;
};


const formatDuration = (totalSeconds: number) => {
    const safeSeconds = clampMin(Math.floor(totalSeconds), 0);
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
    const minutes = clampMin(Math.floor(elapsedMinutes), 1);

    const safePricePerMinute = Math.max(0, pricePerMinute);

    if (billingType === BillingTypeEnum.PerMinute) {
        return {
            total: minutes * safePricePerMinute,
            chargedMinutes: minutes,
            breakdown: `${formatMoneyByN(safePricePerMinute)} / мин × ${minutes} мин`,
        };
    }

    const pricePerHour = safePricePerMinute * 60;
    const hours = clampMin(Math.ceil(minutes / 60), 1);

    return {
        total: hours * pricePerHour,
        chargedHours: hours,
        breakdown: `${formatMoneyByN(pricePerHour)} / час × ${hours} ч`,
    };
};

export const ActiveVisitPage = () => {
    const dispatch = useDispatch<AppDispatch>();
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const userId = useSelector((state: RootState) => state.auth.userId);
    const visitStatus = useSelector((state: RootState) => state.visit.status);
    const activeVisit = useSelector((state: RootState) => state.visit.activeVisit);
    const loadingActiveVisit = useSelector((state: RootState) => state.visit.loadingActiveVisit);
    const endingVisit = useSelector((state: RootState) => state.visit.endingVisit);
    const visitError = useSelector((state: RootState) => state.visit.error);

    const [now, setNow] = useState(() => Date.now());
    const [confirmOpen, setConfirmOpen] = useState(false);
    const [exitComplete, setExitComplete] = useState(false);

    useEffect(() => {
        if (!userId) return;
        void dispatch(loadActiveVisitByUser({userId}));
    }, [dispatch, userId]);

    useEffect(() => {
        if (loadingActiveVisit || visitError) return;
        if (visitStatus !== VisitUiStatus.Active || !activeVisit) {
            navigate("/visit/start", {replace: true});
        }
    }, [activeVisit, loadingActiveVisit, navigate, visitError, visitStatus]);

    useEffect(() => {
        const id = window.setInterval(() => setNow(Date.now()), 1000);
        return () => window.clearInterval(id);
    }, []);

    const elapsedSeconds = useMemo(() => {
        const startedAtMs = activeVisit?.startedAtMs ?? now;
        const delta = now - startedAtMs;
        return Math.max(0, Math.floor(delta / 1000));
    }, [activeVisit?.startedAtMs, now]);

    const elapsedMinutes = useMemo(() => Math.max(1, Math.ceil(elapsedSeconds / 60)), [elapsedSeconds]);

    const estimate = useMemo(() => {
        const billingType = activeVisit?.tariff.billingType ?? BillingTypeEnum.PerMinute;
        const pricePerMinute = activeVisit?.tariff.pricePerMinute ?? 0;
        return calcEstimate(elapsedMinutes, billingType, pricePerMinute);
    }, [activeVisit?.tariff.billingType, activeVisit?.tariff.pricePerMinute, elapsedMinutes]);

    const progressToNextHour = useMemo(() => {
        if (activeVisit?.tariff.billingType !== BillingTypeEnum.Hourly) return null;
        const minutesIntoHour = elapsedMinutes % 60;
        return minutesIntoHour / 60;
    }, [activeVisit?.tariff.billingType, elapsedMinutes]);

    const onFinishVisit = async () => {
        const action = await dispatch(endVisitOnServer({visitId: activeVisit?.visitId}));
        if (endVisitOnServer.rejected.match(action)) {
            showToast(action.payload ?? "Не удалось завершить визит", "error", "Ошибка");
            return;
        }

        setExitComplete(true);
        setConfirmOpen(false);
        showToast("Визит завершён", "success", "Готово");
        navigate("/visit/start", {replace: true});
    };

    const onRetryLoad = async () => {
        if (!userId) return;
        dispatch(clearVisitError());
        await dispatch(loadActiveVisitByUser({userId}));
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
                        {visitError && (
                            <MessageBar intent="error" data-testid="visit-active-error">
                                <MessageBarBody>
                                    <MessageBarTitle>Ошибка загрузки визита</MessageBarTitle>
                                    {visitError}
                                </MessageBarBody>
                                <MessageBarActions>
                                    <Button appearance="secondary" icon={<ArrowClockwise20Regular/>}
                                            data-testid="visit-active-retry"
                                            onClick={() => void onRetryLoad()}>
                                        Повторить
                                    </Button>
                                </MessageBarActions>
                            </MessageBar>
                        )}

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
                                                {activeVisit?.tariff.name ?? "Тариф"}
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
                                                {formatTimeHHmm("", new Date(activeVisit?.startedAtMs ?? now))}
                                            </Title3>
                                        </div>
                                        <div className="flex items-center justify-between gap-3">
                                            <Body1>Тип</Body1>
                                            <Body1>
                                                {activeVisit?.tariff.billingType === BillingTypeEnum.Hourly ? "Почасовой" : "Поминутный"}
                                            </Body1>
                                        </div>
                                    </div>
                                </div>
                            </HoverTiltCard>
                        </div>

                        <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                            <HoverTiltCard className="lg:col-span-8">
                                <div className="flex flex-col gap-4">
                                    <div className="flex items-center gap-2">
                                        <Info20Regular/>
                                        <Subtitle2Stronger>Детали расчёта</Subtitle2Stronger>
                                    </div>
                                    <Divider/>
                                    <div>
                                        <div className="flex flex-col gap-3">
                                            <div className="flex items-center justify-between gap-4 flex-wrap">
                                                <div className="flex items-center gap-2">
                                                    <Sticker20Regular/>
                                                    <Body1 block>Тариф</Body1>
                                                </div>
                                                <Title3 block>{activeVisit?.tariff.name ?? "Тариф"}</Title3>
                                            </div>

                                            <Divider/>

                                            <div className="flex items-center justify-between gap-4 flex-wrap">
                                                <div className="flex items-center gap-2">
                                                    <Money20Regular/>
                                                    <Body1 block>Тип</Body1>
                                                </div>
                                                <Title3 block>
                                                    {activeVisit?.tariff.billingType === BillingTypeEnum.Hourly ? "Почасовой" : "Поминутный"}
                                                </Title3>
                                            </div>

                                            <Divider/>

                                            <div className="flex items-start justify-between gap-4">
                                                <div className="flex items-center gap-2">
                                                    <Info20Regular/>
                                                    <Body1 block>Расчёт</Body1>
                                                </div>

                                                <div className="flex flex-col items-end gap-2 text-right">
                                                    <Title1 block>{formatMoneyByN(estimate.total)}</Title1>
                                                    <Body1 block>{estimate.breakdown}</Body1>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <Body2 block>
                                        {activeVisit?.tariff.billingType === BillingTypeEnum.Hourly
                                            ? "Почасовой: округление вверх до часа."
                                            : "Поминутный: расчёт по минутам."}
                                    </Body2>
                                </div>
                            </HoverTiltCard>

                            <HoverTiltCard className="!hidden lg:!block lg:col-span-4">
                                <div className="flex flex-col gap-4">
                                    <div className="flex items-center gap-2">
                                        <WeatherMoon20Regular/>
                                        <Subtitle2Stronger>Атмосфера</Subtitle2Stronger>
                                    </div>

                                    <Body2 block>
                                        Небольшая панель для полезной информации: правила, Wi‑Fi, розетки, кухня и
                                        быстрые подсказки по тарифу.
                                    </Body2>

                                    <Divider/>

                                    {activeVisit?.tariff.billingType === BillingTypeEnum.Hourly && progressToNextHour !== null ? (
                                        <div className="flex flex-col gap-2">
                                            <Body1 block>Прогресс до следующего часа</Body1>
                                            <ProgressBar value={progressToNextHour}/>
                                            <Body1 block>
                                                {elapsedMinutes % 60} мин из 60
                                            </Body1>
                                        </div>
                                    ) : (
                                        <div className="flex flex-col gap-2">
                                            <Body1 block>Статус визита</Body1>
                                            <ProgressBar/>
                                            <Body1 block>Активен — обновляется в реальном времени</Body1>
                                        </div>
                                    )}
                                </div>
                            </HoverTiltCard>
                        </div>

                        <Dialog open={confirmOpen} onOpenChange={(_, data) => setConfirmOpen(data.open)}>
                            <DialogSurface className="w-[calc(100vw-32px)] max-w-[520px]">
                                <DialogBody>
                                    <DialogTitle data-testid="visit-end-dialog-title">
                                        <div className="flex items-center gap-2">
                                            <Shield20Regular/>
                                            Завершить визит
                                        </div>
                                    </DialogTitle>
                                    <DialogContent>
                                        <div className="flex flex-col gap-4">
                                            <Body1 block>
                                                Подтвердите выход — будет рассчитана финальная стоимость и списан
                                                баланс.
                                            </Body1>
                                            <Body1 block>
                                                Финальная стоимость будет рассчитана и отправлена в биллинг.
                                            </Body1>

                                            <Card>
                                                <div className="flex flex-col gap-3">
                                                    <div className="flex items-center justify-between gap-3">
                                                        <Body1>Тариф</Body1>
                                                        <Body1>{activeVisit?.tariff.name ?? "Тариф"}</Body1>
                                                    </div>
                                                    <div className="flex items-center justify-between gap-3">
                                                        <Body1>Длительность</Body1>
                                                        <Body1>{formatDuration(elapsedSeconds)}</Body1>
                                                    </div>

                                                    <Divider/>

                                                    <div className="flex items-center justify-between gap-3">
                                                        <Body1 block>
                                                            Итого к списанию
                                                        </Body1>
                                                        <Title3>{formatMoneyByN(estimate.total)}</Title3>
                                                    </div>
                                                    <Body1>{estimate.breakdown}</Body1>
                                                </div>
                                            </Card>
                                        </div>
                                    </DialogContent>

                                    <DialogActions>
                                        <Button appearance="secondary" data-testid="visit-end-cancel" onClick={() => setConfirmOpen(false)}>
                                            Отмена
                                        </Button>
                                        <Button
                                            appearance="primary"
                                            data-testid="visit-end-confirm"
                                            disabled={endingVisit}
                                            onClick={() => void onFinishVisit()}
                                        >
                                            {endingVisit ? "Завершение..." : "Завершить визит"}
                                        </Button>
                                    </DialogActions>
                                </DialogBody>
                            </DialogSurface>
                        </Dialog>
                    </div>
                </div>
            </div>
        </div>
    );
};
