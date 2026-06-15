import {createApi} from "@reduxjs/toolkit/query/react";
import {baseQueryWithReauth} from "@store/api/baseQuery";
import type {BillingBalance, BillingTransaction} from "@app-types/billing";
import type {PagedResponse} from "@app-types/pagination";

export interface Invoice {
    invoiceId: string;
    userId: string | null;
    visitId: string;
    totalAmount: number;
    status: number; // 1 = Pending, 2 = Paid, 3 = Cancelled
    paymentMethod: number | null; // 1 = Card, 2 = Cash, 3 = Online
    stripeSessionId: string | null;
    createdAt: string;
    paidAt: string | null;
    fiscalReceiptNumber?: string | null;
    fiscalReceiptUrl?: string | null;
}

export interface GetInvoicesPageArgs {
    page: number;
    pageSize: number;
    userId?: string | null;
}

export interface GetInvoicesPageResponse {
    invoices: Invoice[];
    pagination: {
        currentPage: number;
        pageSize: number;
        totalCount: number;
        totalPages: number;
    };
}

export interface PayInvoiceRequest {
    invoiceId: string;
    method: number; // 1 = Card, 2 = Cash, 3 = Online
}

export interface InitializeStripeInvoicePaymentRequest {
    invoiceId: string;
    successUrl: string;
    cancelUrl: string;
}

export interface InitializeStripeInvoicePaymentResponse {
    checkoutUrl: string;
}

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
    tagTypes: ["Balance", "Debt", "Transactions", "Invoices"],
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

        getBalancesBulk: builder.query<Record<string, number>, string[]>({
            query: (userIds) => ({
                url: "/billing/balance/bulk",
                method: "POST",
                body: userIds,
            }),
            providesTags: ["Balance"],
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

        simulateStripeWebhook: builder.mutation<
            { message: string },
            {
                eventType: string;
                userId: string;
                paymentId: string;
                externalPaymentId: string;
                amount: number;
            }
        >({
            query: (body) => ({
                url: "/billing/stripe/debug-webhook",
                method: "POST",
                body,
            }),
            invalidatesTags: (_result, _error, arg) => [
                {type: "Balance", id: arg.userId},
                {type: "Transactions", id: arg.userId},
                {type: "Debt", id: arg.userId},
            ],
        }),

        getInvoice: builder.query<Invoice, string>({
            query: (invoiceId) => `/billing/invoices/${invoiceId}`,
            providesTags: (_result, _error, invoiceId) => [{type: "Invoices", id: invoiceId}],
        }),

        getInvoiceByVisitId: builder.query<Invoice, string>({
            query: (visitId) => `/billing/invoices/visit/${visitId}`,
            providesTags: (_result, _error, visitId) => [{type: "Invoices", id: visitId}],
        }),

        getInvoicesPage: builder.query<GetInvoicesPageResponse, GetInvoicesPageArgs>({
            query: ({page, pageSize, userId}) => ({
                url: "/billing/admin/invoices",
                params: {page, pageSize, userId: userId || undefined},
            }),
            providesTags: ["Invoices"],
        }),

        payInvoice: builder.mutation<{ message: string }, PayInvoiceRequest>({
            query: ({invoiceId, ...body}) => ({
                url: `/billing/invoices/${invoiceId}/pay`,
                method: "POST",
                body,
            }),
            invalidatesTags: () => [
                "Invoices",
                "Transactions",
                "Balance",
                "Debt"
            ],
        }),

        initializeStripeInvoicePayment: builder.mutation<
            InitializeStripeInvoicePaymentResponse,
            InitializeStripeInvoicePaymentRequest
        >({
            query: ({invoiceId, ...body}) => ({
                url: `/billing/invoices/${invoiceId}/pay-stripe`,
                method: "POST",
                body,
            }),
            invalidatesTags: ["Invoices"],
        }),
    }),
});

export const {
    useGetBalanceQuery,
    useGetDebtQuery,
    useGetTransactionHistoryQuery,
    useInitializeStripeCheckoutMutation,
    useSimulateStripeWebhookMutation,
    useGetInvoiceQuery,
    useGetInvoiceByVisitIdQuery,
    useGetInvoicesPageQuery,
    usePayInvoiceMutation,
    useInitializeStripeInvoicePaymentMutation,
    useGetBalancesBulkQuery,
} = billingApi;
