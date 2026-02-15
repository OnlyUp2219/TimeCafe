import {
    Body1,
    Button,
    Card,
    Divider,
    MessageBar,
    MessageBarActions,
    MessageBarBody,
    MessageBarTitle,
    Text,
    Title2,
    Tooltip,
} from "@fluentui/react-components";
import {useCallback, useEffect, useMemo, useState} from "react";
import {useDispatch, useSelector} from "react-redux";
import {useNavigate} from "react-router-dom";
import type {AppDispatch, RootState} from "@store";
import {
    clearVisitError,
    loadActiveTariffs,
    loadActiveVisitByUser,
    setSelectedTariffId,
    startVisitOnServer,
    VisitUiStatus
} from "@store/visitSlice";
import {BillingType as BillingTypeEnum, type BillingType, type Tariff} from "@app-types/tariff";
import {clamp} from "@utility/clamp";
import {useProgressToast} from "@components/ToastProgress/ToastProgress";
import {DismissRegular} from "@fluentui/react-icons";

import repeatTriangleUrl from "@assets/rrrepeat_triangle.svg";
import vortexUrl from "@assets/vvvortex.svg";
import blob2Url from "@assets/ssshape_blob2.svg";
import blob4Url from "@assets/ssshape_blob4.svg";
import "./visits.css";
import {type CalcResult, TariffForecastCard} from "@components/Tariff/TariffForecastCard.tsx";
import {TariffCarouselSection} from "@components/Tariff/TariffCarouselSection.tsx";
import {VisitParamsCard} from "@components/Tariff/VisitParamsCard.tsx";

const calculate = (minutes: number, pricePerMinute: number, billingType: BillingType): CalcResult => {
    const safeMinutes = clamp(Math.floor(minutes), 1, 12 * 60);
    const priceMinute = Math.max(0, pricePerMinute);
    const priceHour = priceMinute * 60;

    if (billingType === BillingTypeEnum.PerMinute) {
        return {
            total: safeMinutes * priceMinute,
            chargedMinutes: safeMinutes,
            chargedHours: 0,
            pricePerMinute: priceMinute,
            pricePerHour: priceHour,
        };
    }

    const hours = Math.max(1, Math.ceil(safeMinutes / 60));

    return {
        total: hours * priceHour,
        chargedMinutes: safeMinutes,
        chargedHours: hours,
        pricePerMinute: priceMinute,
        pricePerHour: priceHour,
    };
};


export const TariffSelectionPage = () => {
    const dispatch = useDispatch<AppDispatch>();
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const userId = useSelector((state: RootState) => state.auth.userId);
    const selectedTariffId = useSelector((state: RootState) => state.visit.selectedTariffId);
    const visitStatus = useSelector((state: RootState) => state.visit.status);
    const tariffs = useSelector((state: RootState) => state.visit.tariffs);
    const loadingTariffs = useSelector((state: RootState) => state.visit.loadingTariffs);
    const startingVisit = useSelector((state: RootState) => state.visit.startingVisit);
    const visitError = useSelector((state: RootState) => state.visit.error);

    const mockTariffs = useMemo<Tariff[]>(() => tariffs, [tariffs]);

    const visibleTariffs = useMemo(
        () => mockTariffs.filter((tariff): tariff is Tariff => tariff !== null && tariff.isActive),
        [mockTariffs]
    );

    const selectedTariff = useMemo(
        () => mockTariffs.find((t) => t.tariffId === selectedTariffId) ?? null,
        [mockTariffs, selectedTariffId]
    );

    const initialActiveIndex = useMemo(() => {
        const idx = visibleTariffs.findIndex((t) => t.tariffId === selectedTariffId);
        return idx >= 0 ? idx : 0;
    }, [visibleTariffs, selectedTariffId]);

    const [activeIndex, setActiveIndex] = useState<number>(initialActiveIndex);
    const [durationMinutes, setDurationMinutes] = useState<number>(90);

    const calc = useMemo(() => {
        if (!selectedTariff) return null;
        return calculate(durationMinutes, selectedTariff.pricePerMinute, selectedTariff.billingType);
    }, [durationMinutes, selectedTariff]);

    const setActiveTariff = useCallback(
        (index: number) => {
            const safeIndex = clamp(index, 0, Math.max(0, visibleTariffs.length - 1));
            setActiveIndex(safeIndex);
        },
        [visibleTariffs.length]
    );

    const onSelectTariff = useCallback(
        (tariffId: string) => {
            const idx = visibleTariffs.findIndex((t) => t.tariffId === tariffId);
            dispatch(setSelectedTariffId(tariffId));
            if (idx >= 0) setActiveTariff(idx);
        },
        [dispatch, setActiveTariff, visibleTariffs]
    );

    useEffect(() => {
        setActiveIndex(initialActiveIndex);
    }, [initialActiveIndex]);

    useEffect(() => {
        void dispatch(loadActiveTariffs());
    }, [dispatch]);

    useEffect(() => {
        if (!userId) return;
        void dispatch(loadActiveVisitByUser({userId}));
    }, [dispatch, userId]);

    useEffect(() => {
        if (!selectedTariffId && visibleTariffs.length > 0) {
            dispatch(setSelectedTariffId(visibleTariffs[0].tariffId));
        }
    }, [dispatch, selectedTariffId, visibleTariffs]);

    useEffect(() => {
        if (visitStatus === VisitUiStatus.Active) {
            navigate("/visit/active", {replace: true});
        }
    }, [navigate, visitStatus]);

    const presets = useMemo(() => [30, 60, 90, 120, 180, 240], []);

    const onStartVisit = useCallback(async () => {
        if (!selectedTariff) return;
        const action = await dispatch(startVisitOnServer({
            tariffId: selectedTariff.tariffId,
            plannedMinutes: durationMinutes,
            userId,
        }));

        if (startVisitOnServer.rejected.match(action)) {
            showToast(action.payload ?? "Не удалось начать визит", "error", "Ошибка");
            return;
        }

        navigate("/visit/active");
    }, [dispatch, durationMinutes, navigate, selectedTariff, showToast, userId]);

    const onRetryLoad = useCallback(async () => {
        dispatch(clearVisitError());
        await dispatch(loadActiveTariffs());
        if (userId) {
            await dispatch(loadActiveVisitByUser({userId}));
        }
    }, [dispatch, userId]);

    const showEmptyTariffs = !loadingTariffs && visibleTariffs.length === 0;

    return (
        <div className="tc-noise-overlay relative overflow-hidden min-h-full">
            {ToasterElement}
            <div className="pointer-events-none absolute inset-0 z-0 overflow-hidden">
                <img
                    src={repeatTriangleUrl}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[8vw] -left-[10vw] w-[60vw] max-w-[720px] -rotate-6 select-none opacity-[0.1]"
                    draggable={false}
                />
                <img
                    src={blob2Url}
                    alt=""
                    aria-hidden="true"
                    className="absolute -right-[14vw] top-[18vh] w-[55vw] max-w-[720px] rotate-6 select-none opacity-[0.1]"
                    draggable={false}
                />
                <img
                    src={blob4Url}
                    alt=""
                    aria-hidden="true"
                    className="absolute -left-[14vw] top-[70vh] w-[60vw] max-w-[760px] -rotate-6 select-none opacity-[0.1]"
                    draggable={false}
                />
                <img
                    src={vortexUrl}
                    alt=""
                    aria-hidden="true"
                    className="absolute -right-[12vw] top-[72vh] w-[60vw] max-w-[720px] select-none lg:top-[56rem] opacity-[0.1]"
                    draggable={false}
                />
            </div>

            <div className="mx-auto w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6 relative z-10">
                <div className="rounded-3xl p-5 sm:p-8 tc-visits-panel">
                    <div className="flex flex-col gap-4">
                        {visitError && (
                            <MessageBar intent="error">
                                <MessageBarBody>
                                    <MessageBarTitle>Ошибка загрузки</MessageBarTitle>
                                    {visitError}
                                </MessageBarBody>
                                <MessageBarActions
                                    containerAction={
                                        <Button
                                            appearance="transparent"
                                            aria-label="Закрыть"
                                            icon={<DismissRegular className="size-5"/>}
                                            onClick={() => dispatch(clearVisitError())}
                                        />
                                    }
                                />
                            </MessageBar>
                        )}

                        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                            <div className="min-w-0">
                                <div className="flex items-center gap-2 flex-wrap">
                                    <Title2>Выбор тарифа</Title2>
                                </div>
                                <Body1 block className="mt-2">
                                    Выберите тариф в карусели, затем задайте примерное время пребывания.
                                </Body1>
                            </div>
                        </div>

                        <Divider/>

                        {loadingTariffs && visibleTariffs.length === 0 ? (
                            <Card>
                                <Body1 block>Загружаем доступные тарифы...</Body1>
                            </Card>
                        ) : showEmptyTariffs ? (
                            <Card className="flex flex-col gap-3">
                                <Body1 block>Сейчас нет доступных тарифов. Попробуйте обновить список.</Body1>
                                <Button appearance="primary" onClick={() => void onRetryLoad()}>
                                    Обновить тарифы
                                </Button>
                            </Card>
                        ) : (
                            <TariffCarouselSection
                                visibleTariffs={visibleTariffs}
                                totalCount={visibleTariffs.length}
                                activeIndex={activeIndex}
                                onActiveIndexChange={setActiveTariff}
                                selectedTariffId={selectedTariffId}
                                onSelectTariff={onSelectTariff}
                            />
                        )}

                        <Divider/>

                        <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                            <VisitParamsCard
                                selectedTariff={selectedTariff}
                                durationMinutes={durationMinutes}
                                setDurationMinutes={setDurationMinutes}
                                presets={presets}
                            />

                            <TariffForecastCard selectedTariff={selectedTariff} calc={calc}/>
                        </div>

                        <div className="flex flex-col gap-2">
                            <Tooltip content="Начать визит" relationship="label">
                                <Button
                                    appearance="primary"
                                    className="w-full"
                                    disabled={!selectedTariff || loadingTariffs || startingVisit || showEmptyTariffs}
                                    onClick={() => void onStartVisit()}
                                >
                                    <Text truncate wrap={false}>{startingVisit ? "Запуск..." : "Начать визит"}</Text>
                                </Button>
                            </Tooltip>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};
