import {createSlice, type PayloadAction} from "@reduxjs/toolkit";
import type {Tariff} from "../types/tariff";
import type {BillingFilters, BillingPagination, BillingTransaction} from "../types/billing";
import {mockBilling, mockTariffs} from "../pages/billing/billing.mock";

export type BillingState = {
    balanceRub: number;
    debtRub: number;

    tariffs: Tariff[];
    lastVisitTariffId: string | null;

    transactions: BillingTransaction[];
    filters: BillingFilters;
    pagination: BillingPagination;
};

const initialState: BillingState = {
    balanceRub: mockBilling.balanceRub,
    debtRub: mockBilling.debtRub,

    tariffs: mockTariffs,
    lastVisitTariffId: mockBilling.lastVisitTariffId,

    transactions: [],
    filters: {period: "month"},
    pagination: {page: 1, pageSize: 20},
};

const billingSlice = createSlice({
    name: "billing",
    initialState,
    reducers: {
        setBalanceRub: (state, action: PayloadAction<number>) => {
            state.balanceRub = action.payload;
        },
        setDebtRub: (state, action: PayloadAction<number>) => {
            state.debtRub = action.payload;
        },
        setTariffs: (state, action: PayloadAction<Tariff[]>) => {
            state.tariffs = action.payload;
        },
        setLastVisitTariffId: (state, action: PayloadAction<string | null>) => {
            state.lastVisitTariffId = action.payload;
        },
        setTransactions: (state, action: PayloadAction<BillingTransaction[]>) => {
            state.transactions = action.payload;
        },
        setFilters: (state, action: PayloadAction<BillingFilters>) => {
            state.filters = action.payload;
            state.pagination.page = 1;
        },
        setPagination: (state, action: PayloadAction<BillingPagination>) => {
            state.pagination = action.payload;
        },
    },
});

export const {
    setBalanceRub,
    setDebtRub,
    setTariffs,
    setLastVisitTariffId,
    setTransactions,
    setFilters,
    setPagination,
} = billingSlice.actions;

export default billingSlice.reducer;
