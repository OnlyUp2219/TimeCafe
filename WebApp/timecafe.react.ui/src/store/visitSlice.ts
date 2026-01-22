import {createSlice, type PayloadAction} from "@reduxjs/toolkit";
import type {Tariff} from "../types/tariff";

export type VisitStatus = "idle" | "active" | "finished";

export type ActiveVisit = {
    startedAtMs: number;
    plannedMinutes: number;
    tariff: Tariff;
};

type VisitState = {
    selectedTariffId: string | null;
    status: VisitStatus;
    activeVisit: ActiveVisit | null;
};

export const initialVisitState: VisitState = {
    selectedTariffId: "standard",
    status: "idle",
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
            state.status = "active";
            state.activeVisit = {
                startedAtMs: Date.now(),
                plannedMinutes: action.payload.plannedMinutes,
                tariff: action.payload.tariff,
            };
            state.selectedTariffId = action.payload.tariff.tariffId;
        },
        finishVisit: (state) => {
            state.status = "finished";
            state.activeVisit = null;
        },
        resetVisit: (state) => {
            state.status = "idle";
            state.activeVisit = null;
        },
    },
});

export const {setSelectedTariffId, clearSelectedTariffId, startVisit, finishVisit, resetVisit} = visitSlice.actions;

export default (state: VisitState | undefined, action: unknown) =>
    visitSlice.reducer((state ?? initialVisitState) as VisitState, action);
