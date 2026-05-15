import {createApi} from "@reduxjs/toolkit/query/react";
import {baseQueryWithReauth} from "@store/api/baseQuery";
import type {BillingBalance, BillingTransaction} from "@app-types/billing";
import type {PagedResponse} from "@app-types/pagination";

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

type GetTransactionHistoryResponse = PagedResponse<BillingTransaction>;

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
            query: (userId) => `/billing/balance/${userId}/debt`,
            transformResponse: (response: GetDebtResponse) => response.debt,
            providesTags: (_result, _error, userId) => [{type: "Debt", id: userId}],
        }),

        getTransactionHistory: builder.query<
            GetTransactionHistoryResponse,
            GetTransactionHistoryArgs
        >({
            query: ({userId, page, pageSize}) => ({
                url: `/billing/transactions/history/${userId}`,
                params: {page, pageSize},
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
