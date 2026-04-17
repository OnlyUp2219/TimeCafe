import {createApi} from "@reduxjs/toolkit/query/react";
import {baseQueryWithReauth} from "@store/api/baseQuery";
import type {User} from "@app-types/user";
import type {BillingTransaction} from "@app-types/billing";

export interface RoleDto {
    name: string;
    normalizedName: string;
}

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

        getRoleClaimsByName: builder.query<{roleClaim: {roleName: string; claims: string[]}}, string>({
            query: (roleName) => `/auth/rbac/role-claims/${roleName}`,
            providesTags: (_result, _error, roleName) => [{type: "Users", id: `claims-${roleName}`}],
        }),

        updateRoleClaims: builder.mutation<{message: string}, {roleName: string; claims: string[]}>({
            query: ({roleName, claims}) => ({
                url: `/auth/rbac/role-claims/${roleName}`,
                method: "PUT",
                body: {claims},
            }),
            invalidatesTags: (_result, _error, arg) => [{type: "Users", id: `claims-${arg.roleName}`}],
        }),

        assignRoleToUser: builder.mutation<{message: string}, {userId: string; roleName: string}>({
            query: ({userId, roleName}) => ({
                url: `/auth/rbac/users/${userId}/roles/${roleName}`,
                method: "POST",
            }),
            invalidatesTags: ["Users"],
        }),

        removeRoleFromUser: builder.mutation<{message: string}, {userId: string; roleName: string}>({
            query: ({userId, roleName}) => ({
                url: `/auth/rbac/users/${userId}/roles/${roleName}`,
                method: "DELETE",
            }),
            invalidatesTags: ["Users"],
        }),

        getPermissions: builder.query<{permissions: string[]}, void>({
            query: () => "/auth/rbac/permissions",
            providesTags: ["Users"],
        }),

        getRoles: builder.query<{roles: RoleDto[]}, void>({
            query: () => "/auth/rbac/roles",
            providesTags: ["Users"],
        }),

        createRole: builder.mutation<{message: string}, {roleName: string}>({
            query: (body) => ({
                url: "/auth/rbac/roles",
                method: "POST",
                body,
            }),
            invalidatesTags: ["Users"],
        }),

        deleteRole: builder.mutation<{message: string}, string>({
            query: (roleName) => ({
                url: `/auth/rbac/roles/${roleName}`,
                method: "DELETE",
            }),
            invalidatesTags: ["Users"],
        }),

        getUserById: builder.query<{user: User}, string>({
            query: (userId) => `/auth/admin/users/${userId}`,
            providesTags: (_result, _error, id) => [{type: "Users", id}],
        }),

        updateUser: builder.mutation<{message: string}, {userId: string; email?: string; userName?: string; emailConfirmed?: boolean; lockoutEnabled?: boolean; lockoutEnd?: string | null}>({
            query: ({userId, ...body}) => ({
                url: `/auth/admin/users/${userId}`,
                method: "PUT",
                body,
            }),
            invalidatesTags: (_result, _error, arg) => ["Users", {type: "Users", id: arg.userId}],
        }),

        deleteUser: builder.mutation<{message: string}, string>({
            query: (userId) => ({
                url: `/auth/admin/users/${userId}`,
                method: "DELETE",
            }),
            invalidatesTags: ["Users"],
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
    useGetRolesQuery,
    useCreateRoleMutation,
    useDeleteRoleMutation,
    useGetRoleClaimsByNameQuery,
    useUpdateRoleClaimsMutation,
    useAssignRoleToUserMutation,
    useRemoveRoleFromUserMutation,
    useGetPermissionsQuery,
    useGetUserByIdQuery,
    useUpdateUserMutation,
    useDeleteUserMutation,
    useGetAdminBalancesQuery,
    useGetAdminTransactionsQuery,
    useGetAdminPaymentsQuery,
} = adminApi;