import axios, {AxiosHeaders, type AxiosHeaderValue, type AxiosInstance, type AxiosRequestConfig, type InternalAxiosRequestConfig} from "axios";
import {normalizeAxiosError} from "./errors/normalize";

type GetAccessToken = () => string | null;
type SetAccessToken = (token: string | null) => void;

export interface HttpClientAuthHandlers {
    onUnauthorized?: () => void;
    onForbidden?: () => void;
}

export interface HttpClientConfig {
    baseURL?: string;
    getAccessToken: GetAccessToken;
    setAccessToken: SetAccessToken;
    refreshAccessToken?: () => Promise<string | null>;
    handlers?: HttpClientAuthHandlers;
}

const defaultBaseURL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7268";

export const rawHttpClient: AxiosInstance = axios.create({
    baseURL: defaultBaseURL,
    withCredentials: true,
    headers: {
        "Content-Type": "application/json",
    },
});

export const httpClient: AxiosInstance = axios.create({
    baseURL: defaultBaseURL,
    withCredentials: true,
    headers: {
        "Content-Type": "application/json",
    },
});

let requestInterceptorId: number | null = null;
let responseInterceptorId: number | null = null;
let refreshPromise: Promise<string | null> | null = null;

type RetryableAxiosRequestConfig = AxiosRequestConfig & {
    _manualRefresh?: boolean;
    _retry?: boolean;
};

export const configureHttpClient = (config: HttpClientConfig) => {
    if (config.baseURL) {
        rawHttpClient.defaults.baseURL = config.baseURL;
        httpClient.defaults.baseURL = config.baseURL;
    }

    if (requestInterceptorId !== null) {
        httpClient.interceptors.request.eject(requestInterceptorId);
    }

    if (responseInterceptorId !== null) {
        httpClient.interceptors.response.eject(responseInterceptorId);
    }

    requestInterceptorId = httpClient.interceptors.request.use((req: InternalAxiosRequestConfig) => {
        const token = config.getAccessToken();
        if (token) {
            if (!req.headers) {
                req.headers = new AxiosHeaders();
            } else if (!(req.headers instanceof AxiosHeaders)) {
                req.headers = new AxiosHeaders(req.headers as unknown as Record<string, AxiosHeaderValue>);
            }

            req.headers.set("Authorization", `Bearer ${token}`);
        }
        return req;
    });

    responseInterceptorId = httpClient.interceptors.response.use(
        r => r,
        async (err: unknown) => {
            if (!axios.isAxiosError(err)) {
                return Promise.reject(err);
            }

            const status = err.response?.status;
            const originalRequest = err.config as RetryableAxiosRequestConfig;

            if (status === 403) {
                config.handlers?.onForbidden?.();
                return Promise.reject(normalizeAxiosError(err));
            }

            if (status !== 401) {
                return Promise.reject(normalizeAxiosError(err));
            }

            if (!originalRequest || originalRequest._manualRefresh) {
                config.handlers?.onUnauthorized?.();
                return Promise.reject(normalizeAxiosError(err));
            }

            if (!config.refreshAccessToken) {
                config.handlers?.onUnauthorized?.();
                return Promise.reject(normalizeAxiosError(err));
            }

            if (originalRequest._retry) {
                config.handlers?.onUnauthorized?.();
                return Promise.reject(normalizeAxiosError(err));
            }

            originalRequest._retry = true;

            try {
                refreshPromise ??= config.refreshAccessToken();
                const newToken = await refreshPromise;
                refreshPromise = null;

                if (!newToken) {
                    config.setAccessToken(null);
                    config.handlers?.onUnauthorized?.();
                    return Promise.reject(normalizeAxiosError(err));
                }

                config.setAccessToken(newToken);

                if (!originalRequest.headers) {
                    originalRequest.headers = new AxiosHeaders();
                } else if (!(originalRequest.headers instanceof AxiosHeaders)) {
                    originalRequest.headers = new AxiosHeaders(originalRequest.headers as unknown as Record<string, AxiosHeaderValue>);
                }

                (originalRequest.headers as AxiosHeaders).set("Authorization", `Bearer ${newToken}`);

                return httpClient(originalRequest);
            } catch {
                refreshPromise = null;
                config.setAccessToken(null);
                config.handlers?.onUnauthorized?.();
                return Promise.reject(normalizeAxiosError(err));
            }
        }
    );
};
