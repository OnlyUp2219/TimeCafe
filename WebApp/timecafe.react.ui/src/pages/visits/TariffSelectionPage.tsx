import {
    Body1,
    Button,
    Card,
    Divider,
    Title2,
    Tooltip,
    Subtitle2Stronger
} from "@fluentui/react-components";
import { Info20Regular } from "@fluentui/react-icons";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useAppDispatch, useAppSelector } from "@store/hooks";
import { useNavigate } from "react-router-dom";
import {
    setSelectedTariffId,
} from "@store/visitSlice";
import { BillingType as BillingTypeEnum, type BillingType, type Tariff } from "@app-types/tariff";
import { clamp } from "@utility/clamp";
import { useProgressToast } from "@components/ToastProgress/ToastProgress";
import { useComponentSize } from "@hooks/useComponentSize";
import { calcVisitEstimate } from "@utility/visitEstimate";

import "./visits.css";
import { type CalcResult, TariffForecastCard } from "@components/Tariff/TariffForecastCard";
import { TariffCarouselSection } from "@components/Tariff/TariffCarouselSection";
import { VisitParamsCard } from "@components/Tariff/VisitParamsCard";
import { TariffDetailsDialog } from "@components/Tariff/TariffDetailsDialog";
import { useGetActiveTariffsQuery, useHasActiveVisitQuery, useCreateVisitMutation, useGetResourcesQuery, useGetActiveVisitsQuery } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";

const calculate = (
    minutes: number,
    pricePerMinute: number,
    billingType: BillingType,
    minSessionMinutes: number | null = null,
    roundingRule: string | null = null
): CalcResult => {
    const safeMinutes = clamp(Math.floor(minutes), 1, 12 * 60);
    const estimate = calcVisitEstimate(
        safeMinutes,
        billingType,
        pricePerMinute,
        minSessionMinutes,
        roundingRule
    );

    return {
        total: estimate.total,
        chargedMinutes: estimate.chargedMinutes ?? safeMinutes,
        chargedHours: estimate.chargedHours ?? Math.max(1, Math.ceil(safeMinutes / 60)),
        pricePerMinute: pricePerMinute,
        pricePerHour: pricePerMinute * 60,
    };
};


export const TariffSelectionPage = () => {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const { showToast, ToasterElement } = useProgressToast();
    const { sizes } = useComponentSize();
    const userId = useAppSelector((state) => state.auth.userId);
    const selectedTariffId = useAppSelector((state) => state.visit.selectedTariffId);

    const { data: tariffsData, isLoading: loadingTariffs, refetch: refetchTariffs } = useGetActiveTariffsQuery();
    const { data: hasActive, isFetching: isFetchingHasActive } = useHasActiveVisitQuery(userId ?? "", { skip: !userId });
    const [createVisit, { isLoading: startingVisit }] = useCreateVisitMutation();
    const { data: resourcesData } = useGetResourcesQuery();
    const { data: activeVisitsData } = useGetActiveVisitsQuery();

    const [detailsOpen, setDetailsOpen] = useState(false);
    const [selectedTariffIdForDetails, setSelectedTariffIdForDetails] = useState<string | null>(null);
    const [selectedResourceId, setSelectedResourceId] = useState<string | null>(null);

    const handleOpenDetails = useCallback((tariff: Tariff) => {
        setSelectedTariffIdForDetails(tariff.tariffId);
        setDetailsOpen(true);
    }, []);

    const tariffsList = useMemo<Tariff[]>(() => {
        if (!tariffsData) return [];
        return tariffsData.map((t) => ({
            tariffId: t.tariffId,
            name: t.name,
            description: t.description ?? "",
            billingType: t.billingType,
            pricePerMinute: t.pricePerMinute,
            isActive: t.isActive,
            themeName: t.themeName,
            themeEmoji: t.themeEmoji ?? null,
            themeColors: t.themeColors ?? null,
            colors: t.themeColors ?? null,
            maxGuests: t.maxGuests,
            minSessionMinutes: t.minSessionMinutes,
            roundingRule: t.roundingRule,
            audienceTags: t.audienceTags,
        }));
    }, [tariffsData]);

    const visibleTariffs = useMemo(
        () => tariffsList.filter((tariff): tariff is Tariff => tariff?.isActive),
        [tariffsList]
    );

    const selectedTariff = useMemo(
        () => tariffsList.find((t) => t.tariffId === selectedTariffId) ?? null,
        [tariffsList, selectedTariffId]
    );

    const initialActiveIndex = useMemo(() => {
        const idx = visibleTariffs.findIndex((t) => t.tariffId === selectedTariffId);
        return Math.max(idx, 0);
    }, [visibleTariffs, selectedTariffId]);

    const [activeIndex, setActiveIndex] = useState<number>(initialActiveIndex);
    const [durationMinutes, setDurationMinutes] = useState<number>(90);
    const [guestsCount, setGuestsCount] = useState<number>(1);

    const freeResources = useMemo(() => {
        if (!resourcesData) return [];
        const activeResourceIds = new Set(
            (activeVisitsData ?? [])
                .map((v) => v.resourceId)
                .filter((id): id is string => !!id)
        );
        return resourcesData.filter(
            (res) => res.isActive && !activeResourceIds.has(res.resourceId)
        );
    }, [resourcesData, activeVisitsData]);

    const calc = useMemo(() => {
        if (!selectedTariff) return null;
        return calculate(
            durationMinutes,
            selectedTariff.pricePerMinute,
            selectedTariff.billingType,
            selectedTariff.minSessionMinutes,
            selectedTariff.roundingRule
        );
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
        if (visibleTariffs.length > 0) {
            const isValid = visibleTariffs.some((t) => t.tariffId === selectedTariffId);
            if (!isValid) {
                dispatch(setSelectedTariffId(visibleTariffs[0].tariffId));
            }
        }
    }, [dispatch, selectedTariffId, visibleTariffs]);

    useEffect(() => {
        setSelectedResourceId(null);
    }, [selectedTariffId]);

    useEffect(() => {
        if (selectedTariff) {
            const maxGuests = selectedTariff.maxGuests ?? 10;
            if (guestsCount > maxGuests) {
                setGuestsCount(maxGuests);
            }
        }
    }, [selectedTariff, guestsCount]);

    useEffect(() => {
        if (hasActive && !isFetchingHasActive) {
            navigate("/visit/active", { replace: true });
        }
    }, [navigate, hasActive, isFetchingHasActive]);

    const presets = useMemo(() => [30, 60, 90, 120, 180, 240], []);

    const onStartVisit = useCallback(async () => {
        if (!selectedTariff || !userId) return;
        try {
            await createVisit({
                tariffId: selectedTariff.tariffId,
                plannedMinutes: durationMinutes,
                guestsCount,
                userId,
                requirePositiveBalance: true,
                requireEnoughForPlanned: false,
                resourceId: selectedResourceId,
            }).unwrap();
            navigate("/visit/active");
            showToast("Заявка на визит отправлена. Ожидайте подтверждения менеджера.", "info", "Визит");
        } catch (err) {
            const message = getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось начать визит";
            showToast(message, "error", "Ошибка");
        }
    }, [createVisit, durationMinutes, guestsCount, navigate, selectedTariff, showToast, userId, selectedResourceId]);

    const onRetryLoad = useCallback(async () => {
        await refetchTariffs();
    }, [refetchTariffs]);

    const showEmptyTariffs = !loadingTariffs && visibleTariffs.length === 0;

    let tariffContent;
    if (loadingTariffs && visibleTariffs.length === 0) {
        tariffContent = (
            <Card data-testid="visit-start-loading">
                <Body1 block>Загружаем доступные тарифы...</Body1>
            </Card>
        );
    } else if (showEmptyTariffs) {
        tariffContent = (
            <Card className="flex flex-col gap-3" data-testid="visit-start-empty">
                <Body1 block>Сейчас нет доступных тарифов. Попробуйте обновить список.</Body1>
                <Button appearance="primary" size={sizes.button} onClick={() => void onRetryLoad()} data-testid="visit-start-retry">
                    Обновить тарифы
                </Button>
            </Card>
        );
    } else {
        tariffContent = (
            <TariffCarouselSection
                visibleTariffs={visibleTariffs}
                totalCount={visibleTariffs.length}
                activeIndex={activeIndex}
                onActiveIndexChange={setActiveTariff}
                selectedTariffId={selectedTariffId}
                onSelectTariff={onSelectTariff}
                onOpenDetails={handleOpenDetails}
            />
        );
    }

    return (
        <div className="page-content flex flex-col gap-4">
            {ToasterElement}

            <div className="flex flex-col gap-4">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                    <div className="min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                            <Title2>Выбор тарифа</Title2>
                        </div>
                        <div className="flex mt-2">
                            <Body1>
                                Выберите тариф в карусели, затем задайте примерное время пребывания.
                            </Body1>
                        </div>
                    </div>
                </div>

                <Divider />

                {tariffContent}

                <Divider />

                {/* TODO: Make for skills */}
                <div className="bg-blue-500/10 border border-blue-500/20 text-blue-500 rounded-2xl p-4 flex items-start gap-3">
                    <Info20Regular className="text-xl shrink-0 mt-0.5" />
                    <div className="flex flex-col gap-1">
                        <Subtitle2Stronger className="block text-sm">Правила тарификации и округления</Subtitle2Stronger>
                        <Body1 style={{ color: 'var(--colorNeutralForeground2)' }}>
                            При выборе поминутного тарифа списание происходит строго посекундно. Почасовые тарифы округляются до полного часа в большую сторону.
                            Если в тарифе установлен стоп-чек, итоговая стоимость визита не превысит этот лимит, сколько бы времени вы ни провели в заведении.
                        </Body1>
                    </div>
                </div>

                <div className="grid grid-cols-1 gap-4 lg:grid-cols-12">
                    <VisitParamsCard
                        selectedTariff={selectedTariff}
                        durationMinutes={durationMinutes}
                        setDurationMinutes={setDurationMinutes}
                        presets={presets}
                        guestsCount={guestsCount}
                        setGuestsCount={setGuestsCount}
                        resources={freeResources}
                        selectedResourceId={selectedResourceId}
                        setSelectedResourceId={setSelectedResourceId}
                    />

                    <TariffForecastCard selectedTariff={selectedTariff} calc={calc} />
                </div>

                <div className="flex flex-col gap-2">
                    <Tooltip content="Начать визит" relationship="label">
                        <Button
                            appearance="primary"
                            className="w-full"
                            data-testid="visit-start-submit"
                            disabled={!selectedTariff || loadingTariffs || startingVisit || showEmptyTariffs}
                            onClick={() => void onStartVisit()}
                            size={sizes.button}
                        >
                            <Body1 truncate wrap={false}>{startingVisit ? "Запрос..." : "Запросить визит"}</Body1>
                        </Button>
                    </Tooltip>
                </div>
            </div>

            <TariffDetailsDialog
                open={detailsOpen}
                onOpenChange={setDetailsOpen}
                tariffId={selectedTariffIdForDetails} />
        </div >
    );
};
