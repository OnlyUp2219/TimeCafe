import {createApi} from "@reduxjs/toolkit/query/react";
import {baseQueryWithReauth} from "@store/api/baseQuery";
import type {BillingBalance, BillingPagination, BillingTransaction} from "@app-types/billing";

export interface InitializeStripeCheckoutRequest {
    userId: string;
    amount: number;
    successUrl?: string;
    cancelUrl?: string;
    description?: string;
}

export interface InitializeStripeCheckoutResponse {
    paymentId: string;
    sessionId: string;
    checkoutUrl: string;
}

interface GetBalanceResponse {
    balance: BillingBalance;
}

interface GetDebtResponse {
    debt: number;
}

interface GetTransactionHistoryResponse {
    transactions: BillingTransaction[];
    pagination: BillingPagination;
}

export interface GetTransactionHistoryArgs {
    userId: string;
    page: number;
    pageSize: number;
}

export const billingApi = createApi({
    reducerPath: "billingApi",
    baseQuery: baseQueryWithReauth,
    tagTypes: ["Balance", "Debt", "Transactions"],
    endpoints: (builder) => ({
        getBalance: builder.query<BillingBalance, string>({
            query: (userId) => `/billing/balance/${userId}`,
            transformResponse: (response: GetBalanceResponse) => response.balance,
            providesTags: (_result, _error, userId) => [{type: "Balance", id: userId}],
        }),

        getDebt: builder.query<number, string>({
            query: (userId) => `/billing/debt/${userId}`,
            transformResponse: (response: GetDebtResponse) => response.debt,
            providesTags: (_result, _error, userId) => [{type: "Debt", id: userId}],
        }),

        getTransactionHistory: builder.query<
            { transactions: BillingTransaction[]; pagination: BillingPagination },
            GetTransactionHistoryArgs
        >({
            query: ({userId, page, pageSize}) => ({
                url: `/billing/transactions/history/${userId}`,
                params: {page, pageSize},
            }),
            transformResponse: (response: GetTransactionHistoryResponse) => ({
                transactions: response.transactions,
                pagination: response.pagination,
            }),
            providesTags: (_result, _error, arg) => [{type: "Transactions", id: arg.userId}],
        }),

        initializeStripeCheckout: builder.mutation<
            InitializeStripeCheckoutResponse,
            InitializeStripeCheckoutRequest
        >({
            query: (body) => ({
                url: "/billing/payments/initialize-checkout",
                method: "POST",
                body,
            }),
            invalidatesTags: (_result, _error, arg) => [
                {type: "Balance", id: arg.userId},
                {type: "Transactions", id: arg.userId},
            ],
        }),
    }),
});

export const {
    useGetBalanceQuery,
    useGetDebtQuery,
    useGetTransactionHistoryQuery,
    useInitializeStripeCheckoutMutation,
} = billingApi;
