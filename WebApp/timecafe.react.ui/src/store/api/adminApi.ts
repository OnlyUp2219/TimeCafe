import {createApi} from "@reduxjs/toolkit/query/react";
import {baseQueryWithReauth} from "@store/api/baseQuery";
import type {User} from "@app-types/user";

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

export const adminApi = createApi({
    reducerPath: "adminApi",
    baseQuery: baseQueryWithReauth,
    tagTypes: ["Users"],
    endpoints: (builder) => ({
        getUsers: builder.query<GetUsersResponse, GetUsersArgs>({
            query: ({page, size, search, status}) => ({
                url: "/auth/admin/users",
                params: {page, size, search, status},
            }),
            providesTags: ["Users"],
        }),
    }),
});

export const {useGetUsersQuery} = adminApi;