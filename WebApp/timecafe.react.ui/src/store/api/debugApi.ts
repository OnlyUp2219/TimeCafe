import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithReauth } from "@store/api/baseQuery";

export const debugApi = createApi({
    reducerPath: "debugApi",
    baseQuery: baseQueryWithReauth,
    endpoints: (builder) => ({
        getError404Single: builder.query<any, void>({
            query: () => "/auth/debug/error/404-single",
        }),
        getError404Multiple: builder.query<any, void>({
            query: () => "/auth/debug/error/422-multiple",
        }),
        getError500Single: builder.query<any, void>({
            query: () => "/auth/debug/error/500-single",
        }),
        getError500Multiple: builder.query<any, void>({
            query: () => "/auth/debug/error/500-multiple",
        }),
        getErrorException: builder.query<any, void>({
            query: () => "/auth/debug/error/exception",
        }),
        getSuccess: builder.query<any, void>({
            query: () => "/auth/debug/success",
        }),
        getInfo: builder.query<any, void>({
            query: () => "/auth/debug/info",
        }),
        getLegacyResult: builder.query<any, void>({
            query: () => "/auth/debug/legacy-result",
        }),
    }),
});

export const {
    useLazyGetError404SingleQuery,
    useLazyGetError404MultipleQuery,
    useLazyGetError500SingleQuery,
    useLazyGetError500MultipleQuery,
    useLazyGetErrorExceptionQuery,
    useLazyGetSuccessQuery,
    useLazyGetInfoQuery,
    useLazyGetLegacyResultQuery,
} = debugApi;
