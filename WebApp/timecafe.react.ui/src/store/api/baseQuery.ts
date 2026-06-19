import {
    fetchBaseQuery,
    type BaseQueryFn,
    type FetchArgs,
    type FetchBaseQueryError,
} from "@reduxjs/toolkit/query/react";
import type {RootState} from "@store";
import {clearTokens, setAccessToken, setEmail, setRole, setUserId} from "@store/authSlice";
import {getApiBaseUrl} from "@api/apiBaseUrl";
import {getJwtInfo} from "@shared/auth/jwt";

import {tryRefreshAccessToken} from "@shared/auth/refreshToken";

const rawBaseQuery = fetchBaseQuery({
    baseUrl: getApiBaseUrl(),
    credentials: "include",
    timeout: (() => {
        const raw = import.meta.env.VITE_HTTP_TIMEOUT_MS as string | undefined;
        const parsed = raw ? Number(raw) : NaN;
        return Number.isFinite(parsed) && parsed > 0 ? parsed : 15_000;
    })(),
    prepareHeaders: (headers, {getState}) => {
        const token = (getState() as RootState).auth.accessToken;
        if (token) {
            headers.set("Authorization", `Bearer ${token}`);
        }
        return headers;
    },
});

const refreshBaseQuery = fetchBaseQuery({
    baseUrl: getApiBaseUrl(),
    credentials: "include",
});

export const baseQueryWithReauth: BaseQueryFn<string | FetchArgs, unknown, FetchBaseQueryError> = async (
    args,
    api,
    extraOptions,
) => {
    let result = await rawBaseQuery(args, api, extraOptions);

    if (result.error?.status === 401) {
        const newToken = await tryRefreshAccessToken();

        if (newToken) {
            api.dispatch(setAccessToken(newToken));
            const info = getJwtInfo(newToken);
            if (info.userId) api.dispatch(setUserId(info.userId));
            if (info.role) api.dispatch(setRole(info.role));
            if (info.email) api.dispatch(setEmail(info.email));

            result = await rawBaseQuery(args, api, extraOptions);
        } else {
            api.dispatch(clearTokens());
        }
    }

    return result;
};

export {rawBaseQuery, refreshBaseQuery};
