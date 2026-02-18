import {createAsyncThunk, createSlice, type PayloadAction} from "@reduxjs/toolkit";
import type {Tariff} from "@app-types/tariff";
import {VisitStatus, type Visit} from "@app-types/visit.ts";
import type {VisitWithTariff} from "@app-types/visitWithTariff.ts";
import type {TariffWithTheme} from "@app-types/tariffWithTheme.ts";
import {VisitVenueApi} from "@api/venue/visitVenueApi";
import {TariffVenueApi} from "@api/venue/tariffVenueApi";
import {normalizeUnknownError} from "@api/errors/normalize";
import {getUserMessage} from "@api/errors/messages";
import {clearTokens} from "@store/authSlice";

export const VisitUiStatus = {
    Idle: 0,
    Active: VisitStatus.Active,
    Completed: VisitStatus.Completed,
} as const;

export type VisitUiStatus = (typeof VisitUiStatus)[keyof typeof VisitUiStatus];

export type ActiveVisit = {
    visitId: string;
    userId: string;
    startedAtIso: string;
    startedAtMs: number;
    plannedMinutes: number | null;
    tariff: Tariff;
};

type VisitState = {
    data: Visit | null;
    selectedTariffId: string | null;
    status: VisitUiStatus;
    activeVisit: ActiveVisit | null;
    tariffs: Tariff[];
    loadingTariffs: boolean;
    loadingActiveVisit: boolean;
    startingVisit: boolean;
    endingVisit: boolean;
    error: string | null;
    lastCalculatedCost: number | null;
};

export const initialVisitState: VisitState = {
    data: null,
    selectedTariffId: null,
    status: VisitUiStatus.Idle,
    activeVisit: null,
    tariffs: [],
    loadingTariffs: false,
    loadingActiveVisit: false,
    startingVisit: false,
    endingVisit: false,
    error: null,
    lastCalculatedCost: null,
};

type StartVisitPayload = {
    visitId: string;
    userId: string;
    startedAtIso: string;
    plannedMinutes: number | null;
    tariff: Tariff;
};

type VisitThunkState = {
    auth: { userId: string };
    visit: VisitState;
};

const resolveUserId = (state: VisitThunkState, requestedUserId?: string): string => {
    const userId = (requestedUserId ?? state.auth.userId).trim();
    if (!userId) {
        throw new Error("Пользователь не найден");
    }
    return userId;
};

const mapTariffWithThemeToTariff = (item: TariffWithTheme): Tariff => ({
    tariffId: item.tariffId,
    name: item.name,
    description: item.description ?? "",
    billingType: item.billingType,
    pricePerMinute: item.pricePerMinute,
    isActive: item.isActive,
    themeName: item.themeName,
    themeEmoji: item.themeEmoji ?? null,
});

const mapVisitWithTariffToActiveVisit = (
    visit: VisitWithTariff,
    plannedMinutes: number | null
): ActiveVisit => {
    const parsed = Date.parse(visit.entryTime);

    return {
        visitId: visit.visitId,
        userId: visit.userId,
        startedAtIso: visit.entryTime,
        startedAtMs: Number.isNaN(parsed) ? Date.now() : parsed,
        plannedMinutes,
        tariff: {
            tariffId: visit.tariffId,
            name: visit.tariffName,
            description: visit.tariffDescription ?? "",
            billingType: visit.tariffBillingType,
            pricePerMinute: visit.tariffPricePerMinute,
            isActive: visit.status === VisitUiStatus.Active,
        },
    };
};

export const loadActiveTariffs = createAsyncThunk<Tariff[], void, { rejectValue: string }>(
    "visit/loadActiveTariffs",
    async (_, {rejectWithValue}) => {
        try {
            const tariffs = await TariffVenueApi.getActiveTariffs();
            return tariffs.map(mapTariffWithThemeToTariff);
        } catch (e) {
            return rejectWithValue(getUserMessage(normalizeUnknownError(e)));
        }
    }
);

export const loadActiveVisitByUser = createAsyncThunk<
    ActiveVisit | null,
    { userId?: string; plannedMinutes?: number | null } | undefined,
    { state: VisitThunkState; rejectValue: string }
>("visit/loadActiveVisitByUser", async (payload, {getState, rejectWithValue}) => {
    try {
        const state = getState();
        const userId = resolveUserId(state, payload?.userId);
        const hasActiveVisit = await VisitVenueApi.hasActiveVisit(userId);
        if (!hasActiveVisit) {
            return null;
        }
        const visit = await VisitVenueApi.getActiveVisitByUser(userId);
        const plannedMinutes = payload?.plannedMinutes ?? state.visit.activeVisit?.plannedMinutes ?? null;
        return mapVisitWithTariffToActiveVisit(visit, plannedMinutes);
    } catch (e) {
        const normalized = normalizeUnknownError(e);
        if (normalized.statusCode === 404) {
            return null;
        }
        return rejectWithValue(getUserMessage(normalized));
    }
});

export const startVisitOnServer = createAsyncThunk<
    ActiveVisit,
    { tariffId: string; plannedMinutes: number; userId?: string },
    { state: VisitThunkState; rejectValue: string }
>("visit/startVisitOnServer", async (payload, {getState, rejectWithValue}) => {
    try {
        const state = getState();
        const userId = resolveUserId(state, payload.userId);
        const createResult = await VisitVenueApi.createVisit({
            userId,
            tariffId: payload.tariffId,
            plannedMinutes: payload.plannedMinutes,
            requirePositiveBalance: true,
            requireEnoughForPlanned: true,
        });

        const tariffFromState = state.visit.tariffs.find((item) => item.tariffId === payload.tariffId);
        const tariff = tariffFromState ?? await TariffVenueApi.getTariffById(payload.tariffId);

        return {
            visitId: createResult.visit.visitId,
            userId: createResult.visit.userId,
            startedAtIso: createResult.visit.entryTime,
            startedAtMs: Date.parse(createResult.visit.entryTime),
            plannedMinutes: payload.plannedMinutes,
            tariff,
        };
    } catch (e) {
        return rejectWithValue(getUserMessage(normalizeUnknownError(e)));
    }
});

export const endVisitOnServer = createAsyncThunk<
    { visit: Visit; calculatedCost: number },
    { visitId?: string } | undefined,
    { state: VisitThunkState; rejectValue: string }
>("visit/endVisitOnServer", async (payload, {getState, rejectWithValue}) => {
    try {
        const state = getState();
        const visitId = payload?.visitId ?? state.visit.activeVisit?.visitId;
        if (!visitId) {
            return rejectWithValue("Активное посещение не найдено");
        }

        const response = await VisitVenueApi.endVisit(visitId);
        return {
            visit: response.visit,
            calculatedCost: response.calculatedCost,
        };
    } catch (e) {
        return rejectWithValue(getUserMessage(normalizeUnknownError(e)));
    }
});

const visitSlice = createSlice({
    name: "visit",
    initialState: initialVisitState,
    reducers: {
        setSelectedTariffId: (state, action: PayloadAction<string | null>) => {
            state.selectedTariffId = action.payload;
        },
        clearSelectedTariffId: (state) => {
            state.selectedTariffId = null;
        },
        startVisit: (state, action: PayloadAction<StartVisitPayload>) => {
            state.status = VisitUiStatus.Active;
            state.activeVisit = {
                visitId: action.payload.visitId,
                userId: action.payload.userId,
                startedAtIso: action.payload.startedAtIso,
                startedAtMs: Date.parse(action.payload.startedAtIso),
                plannedMinutes: action.payload.plannedMinutes,
                tariff: action.payload.tariff,
            };
            state.selectedTariffId = action.payload.tariff.tariffId;
            state.error = null;
        },
        finishVisit: (state) => {
            state.status = VisitUiStatus.Completed;
            state.activeVisit = null;
            state.error = null;
        },
        resetVisit: (state) => {
            state.status = VisitUiStatus.Idle;
            state.activeVisit = null;
            state.error = null;
            state.lastCalculatedCost = null;
        },
        clearVisitError: (state) => {
            state.error = null;
        }
    },
    extraReducers: (builder) => {
        builder
            .addCase(clearTokens, (state) => {
                state.data = null;
                state.selectedTariffId = null;
                state.status = VisitUiStatus.Idle;
                state.activeVisit = null;
                state.tariffs = [];
                state.loadingTariffs = false;
                state.loadingActiveVisit = false;
                state.startingVisit = false;
                state.endingVisit = false;
                state.error = null;
                state.lastCalculatedCost = null;
            })
            .addCase(loadActiveTariffs.pending, (state) => {
                state.loadingTariffs = true;
                state.error = null;
            })
            .addCase(loadActiveTariffs.fulfilled, (state, action) => {
                state.loadingTariffs = false;
                state.tariffs = action.payload;
                if (!state.selectedTariffId && action.payload.length > 0) {
                    state.selectedTariffId = action.payload[0].tariffId;
                }
            })
            .addCase(loadActiveTariffs.rejected, (state, action) => {
                state.loadingTariffs = false;
                state.error = action.payload ?? "Не удалось получить тарифы";
            })
            .addCase(loadActiveVisitByUser.pending, (state) => {
                state.loadingActiveVisit = true;
                state.error = null;
            })
            .addCase(loadActiveVisitByUser.fulfilled, (state, action) => {
                state.loadingActiveVisit = false;
                state.activeVisit = action.payload;
                state.status = action.payload ? VisitUiStatus.Active : VisitUiStatus.Idle;
                if (action.payload) {
                    state.selectedTariffId = action.payload.tariff.tariffId;
                }
            })
            .addCase(loadActiveVisitByUser.rejected, (state, action) => {
                state.loadingActiveVisit = false;
                if (!state.activeVisit) {
                    state.status = VisitUiStatus.Idle;
                }
                state.error = action.payload ?? "Не удалось получить активный визит";
            })
            .addCase(startVisitOnServer.pending, (state) => {
                state.startingVisit = true;
                state.error = null;
            })
            .addCase(startVisitOnServer.fulfilled, (state, action) => {
                state.startingVisit = false;
                state.activeVisit = action.payload;
                state.status = VisitUiStatus.Active;
                state.selectedTariffId = action.payload.tariff.tariffId;
            })
            .addCase(startVisitOnServer.rejected, (state, action) => {
                state.startingVisit = false;
                state.error = action.payload ?? "Не удалось начать визит";
            })
            .addCase(endVisitOnServer.pending, (state) => {
                state.endingVisit = true;
                state.error = null;
            })
            .addCase(endVisitOnServer.fulfilled, (state, action) => {
                state.endingVisit = false;
                state.data = action.payload.visit;
                state.lastCalculatedCost = action.payload.calculatedCost;
                state.status = VisitUiStatus.Completed;
                state.activeVisit = null;
            })
            .addCase(endVisitOnServer.rejected, (state, action) => {
                state.endingVisit = false;
                state.error = action.payload ?? "Не удалось завершить визит";
            });
    }
});

export const {setSelectedTariffId, clearSelectedTariffId, startVisit, finishVisit, resetVisit, clearVisitError} = visitSlice.actions;

export default visitSlice.reducer;
