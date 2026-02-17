import {createAsyncThunk, createSlice, type PayloadAction} from "@reduxjs/toolkit";
import {
    type BillingBalance,
    type BillingFilters,
    type BillingPagination,
    type BillingTransaction,
} from "@app-types/billing";
import {BillingApi, type InitializeStripeCheckoutResponse} from "@api/billing/billingApi";
import {normalizeUnknownError} from "@api/errors/normalize";
import {getUserMessage} from "@api/errors/messages";
import {clearTokens} from "@store/authSlice";

type BillingThunkState = {
    auth: { userId: string };
    billing: BillingState;
};

const resolveUserId = (state: BillingThunkState, requestedUserId?: string): string => {
    const userId = (requestedUserId ?? state.auth.userId).trim();
    if (!userId) {
        throw new Error("Пользователь не найден");
    }
    return userId;
};

export type BillingState = {
    balance: BillingBalance | null;
    balanceRub: number;
    debtRub: number;
    transactions: BillingTransaction[];
    filters: BillingFilters;
    pagination: BillingPagination;
    loading: boolean;
    loadingTransactions: boolean;
    initializingCheckout: boolean;
    error: string | null;
    checkoutError: string | null;
    checkoutUrl: string | null;
};

const initialState: BillingState = {
    balance: null,
    balanceRub: 0,
    debtRub: 0,

    transactions: [],
    filters: {period: "month"},
    pagination: {currentPage: 1, pageSize: 20, totalCount: 0, totalPages: 0},
    loading: false,
    loadingTransactions: false,
    initializingCheckout: false,
    error: null,
    checkoutError: null,
    checkoutUrl: null,
};

export const loadBillingOverview = createAsyncThunk<
    { balance: BillingBalance; debt: number },
    { userId?: string } | undefined,
    { state: BillingThunkState; rejectValue: string }
>("billing/loadOverview", async (payload, {getState, rejectWithValue}) => {
    try {
        const state = getState();
        const userId = resolveUserId(state, payload?.userId);
        const [balance, debt] = await Promise.all([
            BillingApi.getBalance(userId),
            BillingApi.getDebt(userId),
        ]);

        return {balance, debt};
    } catch (e) {
        return rejectWithValue(getUserMessage(normalizeUnknownError(e)));
    }
});

export const loadBillingTransactions = createAsyncThunk<
    {
        transactions: BillingTransaction[];
        pagination: BillingPagination;
        append: boolean;
    },
    { page?: number; pageSize?: number; append?: boolean; userId?: string } | undefined,
    { state: BillingThunkState; rejectValue: string }
>("billing/loadTransactions", async (payload, {getState, rejectWithValue}) => {
    try {
        const state = getState();
        const userId = resolveUserId(state, payload?.userId);
        const page = payload?.page ?? state.billing.pagination.currentPage;
        const pageSize = payload?.pageSize ?? state.billing.pagination.pageSize;
        const append = payload?.append ?? false;

        const response = await BillingApi.getTransactionHistory(userId, page, pageSize);

        return {
            transactions: response.transactions,
            pagination: response.pagination,
            append,
        };
    } catch (e) {
        return rejectWithValue(getUserMessage(normalizeUnknownError(e)));
    }
});

export const initializeBillingCheckout = createAsyncThunk<
    InitializeStripeCheckoutResponse,
    { amountRub: number; description?: string; userId?: string },
    { state: BillingThunkState; rejectValue: string }
>("billing/initializeCheckout", async (payload, {getState, rejectWithValue}) => {
    try {
        const state = getState();
        const userId = resolveUserId(state, payload.userId);
        const amount = Math.floor(payload.amountRub);
        if (!Number.isFinite(amount) || amount <= 0) {
            return rejectWithValue("Укажите корректную сумму пополнения");
        }

        const baseUrl = typeof window !== "undefined" ? window.location.origin : "";
        const successUrl = baseUrl ? `${baseUrl}/billing?payment=success` : undefined;
        const cancelUrl = baseUrl ? `${baseUrl}/billing?payment=cancel` : undefined;

        return await BillingApi.initializeStripeCheckout({
            userId,
            amount,
            successUrl,
            cancelUrl,
            description: payload.description,
        });
    } catch (e) {
        return rejectWithValue(getUserMessage(normalizeUnknownError(e)));
    }
});

const billingSlice = createSlice({
    name: "billing",
    initialState,
    reducers: {
        setFilters: (state, action: PayloadAction<BillingFilters>) => {
            state.filters = action.payload;
            state.pagination.currentPage = 1;
        },
        setPagination: (state, action: PayloadAction<BillingPagination>) => {
            state.pagination = action.payload;
        },
        clearBillingError: (state) => {
            state.error = null;
            state.checkoutError = null;
        },
        clearCheckoutUrl: (state) => {
            state.checkoutUrl = null;
        },
    },
    extraReducers: (builder) => {
        builder
            .addCase(clearTokens, () => initialState)
            .addCase(loadBillingOverview.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(loadBillingOverview.fulfilled, (state, action) => {
                state.loading = false;
                state.balance = action.payload.balance;
                state.balanceRub = Number(action.payload.balance.currentBalance) || 0;
                state.debtRub = Number(action.payload.debt) || 0;
            })
            .addCase(loadBillingOverview.rejected, (state, action) => {
                state.loading = false;
                state.error = action.payload ?? "Не удалось загрузить баланс";
            })
            .addCase(loadBillingTransactions.pending, (state) => {
                state.loadingTransactions = true;
                state.error = null;
            })
            .addCase(loadBillingTransactions.fulfilled, (state, action) => {
                state.loadingTransactions = false;
                state.transactions = action.payload.append
                    ? [...state.transactions, ...action.payload.transactions]
                    : action.payload.transactions;
                state.pagination = action.payload.pagination;
            })
            .addCase(loadBillingTransactions.rejected, (state, action) => {
                state.loadingTransactions = false;
                state.error = action.payload ?? "Не удалось загрузить историю транзакций";
            })
            .addCase(initializeBillingCheckout.pending, (state) => {
                state.initializingCheckout = true;
                state.checkoutError = null;
            })
            .addCase(initializeBillingCheckout.fulfilled, (state, action) => {
                state.initializingCheckout = false;
                state.checkoutUrl = action.payload.checkoutUrl;
            })
            .addCase(initializeBillingCheckout.rejected, (state, action) => {
                state.initializingCheckout = false;
                state.checkoutError = action.payload ?? "Не удалось инициализировать пополнение";
            });
    },
});

export const {
    setFilters,
    setPagination,
    clearBillingError,
    clearCheckoutUrl,
} = billingSlice.actions;

export default billingSlice.reducer;
