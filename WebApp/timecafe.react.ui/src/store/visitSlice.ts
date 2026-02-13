import {createSlice, type PayloadAction} from "@reduxjs/toolkit";
import type {Tariff} from "@app-types/tariff";
import {VisitStatus, type Visit} from "@app-types/visit.ts";

export const VisitUiStatus = {
    Idle: 0,
    Active: VisitStatus.Active,
    Completed: VisitStatus.Completed,
} as const;

export type VisitUiStatus = (typeof VisitUiStatus)[keyof typeof VisitUiStatus];

export type ActiveVisit = {
    startedAtMs: number;
    plannedMinutes: number;
    tariff: Tariff;
};

type VisitState = {
    data: Visit | null;
    selectedTariffId: string | null;
    status: VisitUiStatus;
    activeVisit: ActiveVisit | null;
};

export const initialVisitState: VisitState = {
    data: null,
    selectedTariffId: "standard",
    status: VisitUiStatus.Idle,
    activeVisit: null,
};

type StartVisitPayload = {
    plannedMinutes: number;
    tariff: Tariff;
};

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
                startedAtMs: Date.now(),
                plannedMinutes: action.payload.plannedMinutes,
                tariff: action.payload.tariff,
            };
            state.selectedTariffId = action.payload.tariff.tariffId;
        },
        finishVisit: (state) => {
            state.status = VisitUiStatus.Completed;
            state.activeVisit = null;
        },
        resetVisit: (state) => {
            state.status = VisitUiStatus.Idle;
            state.activeVisit = null;
        },
    },
});

export const {setSelectedTariffId, clearSelectedTariffId, startVisit, finishVisit, resetVisit} = visitSlice.actions;

export default visitSlice.reducer;
