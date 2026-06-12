import {
    Body1,
    Button,
    Card,
    Divider,
    Title2,
    Tooltip,
} from "@fluentui/react-components";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useAppDispatch, useAppSelector } from "@store/hooks";
import { useNavigate } from "react-router-dom";
import {
    setSelectedTariffId,
} from "@store/visitSlice";
import { type BillingType, type Tariff } from "@app-types/tariff";
import { clamp } from "@utility/clamp";
import { useProgressToast } from "@components/ToastProgress/ToastProgress";
import { useComponentSize } from "@hooks/useComponentSize";
import { calcVisitEstimate } from "@utility/visitEstimate";

import "./visits.css";
import { type CalcResult, TariffForecastCard } from "@components/Tariff/TariffForecastCard";
import { TariffCarouselSection } from "@components/Tariff/TariffCarouselSection";
import { VisitParamsCard } from "@components/Tariff/VisitParamsCard";
import { TariffDetailsDialog } from "@components/Tariff/TariffDetailsDialog";
import { useGetActiveTariffsQuery, useHasActiveVisitQuery, useCreateVisitMutation, useGetResourcesQuery, useGetActiveVisitsQuery, useGetUserLoyaltyQuery, useGetAllPromotionsQuery } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";

const calculate = (
    minutes: number,
    pricePerMinute: number,
    billingType: BillingType,
    minSessionMinutes: number | null = null,
    roundingRule: string | null = null,
    globalDiscount: number = 0,
    tariffDiscount: number = 0,
    personalDiscount: number = 0
): CalcResult => {
    const safeMinutes = clamp(Math.floor(minutes), 1, 12 * 60);
    const estimate = calcVisitEstimate(
        safeMinutes,
        billingType,
        pricePerMinute,
        minSessionMinutes,
        roundingRule,
        globalDiscount,
        tariffDiscount,
        personalDiscount
    );

    return {
        total: estimate.total,
        baseTotal: estimate.baseTotal,
        isDiscounted: estimate.isDiscounted,
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
    const { data: loyalty } = useGetUserLoyaltyQuery(userId ?? "", { skip: !userId });
    const { data: promotions } = useGetAllPromotionsQuery();

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

    const discountsMap = useMemo(() => {
        const map = new Map<string, number>();
        let globalD = 0;
        if (promotions) {
            const now = Date.now();
            const activePromos = promotions.filter(p =>
                p.isActive &&
                (!p.validFrom || new Date(p.validFrom).getTime() <= now) &&
                (!p.validTo || new Date(p.validTo).getTime() >= now)
            );
            const g = activePromos.find(p => !p.tariffId);
            if (g?.discountPercent) globalD = g.discountPercent;

            for (const t of visibleTariffs) {
                let tariffD = 0;
                const tp = activePromos.find(p => p.tariffId === t.tariffId);
                if (tp?.discountPercent) tariffD = tp.discountPercent;
                const personalD = loyalty?.personalDiscountPercent ?? 0;
                const bestPromotion = Math.max(globalD, tariffD);
                const applied = Math.min(bestPromotion + personalD, 50); // max discount 50%
                map.set(t.tariffId, applied);
            }
        }
        return map;
    }, [promotions, loyalty, visibleTariffs]);

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

        let globalD = 0;
        let tariffD = 0;
        if (promotions) {
            const now = Date.now();
            const activePromos = promotions.filter(p =>
                p.isActive &&
                (!p.validFrom || new Date(p.validFrom).getTime() <= now) &&
                (!p.validTo || new Date(p.validTo).getTime() >= now)
            );

            const g = activePromos.find(p => !p.tariffId);
            if (g?.discountPercent) globalD = g.discountPercent;

            const t = activePromos.find(p => p.tariffId === selectedTariff.tariffId);
            if (t?.discountPercent) tariffD = t.discountPercent;
        }

        const personalD = loyalty?.personalDiscountPercent ?? 0;

        return calculate(
            durationMinutes,
            selectedTariff.pricePerMinute,
            selectedTariff.billingType,
            selectedTariff.minSessionMinutes,
            selectedTariff.roundingRule,
            globalD,
            tariffD,
            personalD
        );
    }, [durationMinutes, selectedTariff, promotions, loyalty]);

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
                discountsMap={discountsMap}
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
