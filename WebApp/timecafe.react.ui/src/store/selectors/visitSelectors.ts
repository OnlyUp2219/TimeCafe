import {createSelector} from "@reduxjs/toolkit";
import type {RootState} from "@store";

export const selectVisitStatus = (state: RootState) => state.visit.status;
export const selectActiveVisit = (state: RootState) => state.visit.activeVisit;
export const selectTariffs = (state: RootState) => state.visit.tariffs;
export const selectSelectedTariffId = (state: RootState) => state.visit.selectedTariffId;
export const selectLoadingTariffs = (state: RootState) => state.visit.loadingTariffs;
export const selectLoadingActiveVisit = (state: RootState) => state.visit.loadingActiveVisit;
export const selectStartingVisit = (state: RootState) => state.visit.startingVisit;
export const selectEndingVisit = (state: RootState) => state.visit.endingVisit;
export const selectVisitError = (state: RootState) => state.visit.error;
export const selectLastCalculatedCost = (state: RootState) => state.visit.lastCalculatedCost;

export const selectSelectedTariff = createSelector(
    [selectTariffs, selectSelectedTariffId],
    (tariffs, selectedId) => tariffs.find((t: { tariffId: string }) => t.tariffId === selectedId) ?? null,
);

export const selectVisibleTariffs = createSelector(
    [selectTariffs],
    (tariffs) => tariffs.filter((t: { isActive: boolean }) => t.isActive),
);
