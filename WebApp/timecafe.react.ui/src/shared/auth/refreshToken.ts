import {rawHttpClient} from "@api/httpClient";

let refreshAccessTokenInFlight: Promise<string | null> | null = null;

export const tryRefreshAccessToken = async (): Promise<string | null> => {
    refreshAccessTokenInFlight ??= (async () => {
        try {
            const res = await rawHttpClient.post<{ accessToken: string }>("/auth/refresh-jwt-v2", {}, {
                _manualRefresh: true,
            });
            const token = res.data?.accessToken;
            return token || null;
        } catch {
            return null;
        }
    })();

    try {
        return await refreshAccessTokenInFlight;
    } finally {
        refreshAccessTokenInFlight = null;
    }
};
