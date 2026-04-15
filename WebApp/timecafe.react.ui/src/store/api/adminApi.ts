import {createApi} from "@reduxjs/toolkit/query/react";
import {baseQueryWithReauth} from "@store/api/baseQuery";
import type {User} from "@app-types/user";
import type {BillingBalance, BillingTransaction} from "@app-types/billing";

export interface GetUsersResponse {
    users: User[];
    pagination: {
        currentPage: number;
        pageSize: number;
        totalCount: number;
        totalPages: number;
    };
}

export interface GetUsersArgs {
    page: number;
    size: number;
    search?: string;
    status?: string;
}

interface AdminPagination {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export interface AdminBalanceDto {
    userId: string;
    currentBalance: number;
    totalDeposited: number;
    totalSpent: number;
    debt: number;
    lastUpdated: string;
    createdAt: string;
}

export interface AdminPaymentDto {
    paymentId: string;
    userId: string;
    amount: number;
    paymentMethod: number;
    externalPaymentId: string | null;
    status: number;
    transactionId: string | null;
    createdAt: string;
    completedAt: string | null;
    errorMessage: string | null;
}

export interface GetBalancesPageArgs { page: number; pageSize: number; }
export interface GetTransactionsPageArgs { page: number; pageSize: number; userId?: string; }
export interface GetPaymentsPageArgs { page: number; pageSize: number; userId?: string; }

export const adminApi = createApi({
    reducerPath: "adminApi",
    baseQuery: baseQueryWithReauth,
    tagTypes: ["Users", "AdminBalances", "AdminTransactions", "AdminPayments"],
    endpoints: (builder) => ({
        getUsers: builder.query<GetUsersResponse, GetUsersArgs>({
            query: ({page, size, search, status}) => ({
                url: "/auth/admin/users",
                params: {page, size, search, status},
            }),
            providesTags: ["Users"],
        }),

        getUserById: builder.query<{user: User}, string>({
            query: (userId) => `/auth/admin/users/${userId}`,
            providesTags: (_result, _error, id) => [{type: "Users", id}],
        }),

        getAdminBalances: builder.query<{balances: AdminBalanceDto[]; pagination: AdminPagination}, GetBalancesPageArgs>({
            query: ({page, pageSize}) => ({
                url: "/billing/admin/balances",
                params: {page, pageSize},
            }),
            providesTags: ["AdminBalances"],
        }),

        getAdminTransactions: builder.query<{transactions: BillingTransaction[]; pagination: AdminPagination}, GetTransactionsPageArgs>({
            query: ({page, pageSize, userId}) => ({
                url: "/billing/admin/transactions",
                params: {page, pageSize, userId},
            }),
            providesTags: ["AdminTransactions"],
        }),

        getAdminPayments: builder.query<{payments: AdminPaymentDto[]; pagination: AdminPagination}, GetPaymentsPageArgs>({
            query: ({page, pageSize, userId}) => ({
                url: "/billing/admin/payments",
                params: {page, pageSize, userId},
            }),
            providesTags: ["AdminPayments"],
        }),
    }),
});

export const {
    useGetUsersQuery,
    useGetUserByIdQuery,
    useGetAdminBalancesQuery,
    useGetAdminTransactionsQuery,
    useGetAdminPaymentsQuery,
} = adminApi;