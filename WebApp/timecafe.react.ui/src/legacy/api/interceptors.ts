import axios from "axios";
import {refreshAccessToken} from "./auth.ts";
import {store} from "../../store";
import type {AppDispatch} from "../../store";

let isRefreshing = false;
axios.defaults.withCredentials = true;

axios.interceptors.response.use(
    r => r,
    async error => {
        const originalRequest = error.config as (typeof error.config & {
            _manualRefresh?: boolean;
            _retry?: boolean;
        });
        if (!originalRequest) return Promise.reject(error);

        if (originalRequest.url?.includes("/refresh-jwt-v2") || originalRequest._manualRefresh) {
            return Promise.reject(error);
        }

        if (error.response?.status === 401) {
            if (originalRequest._retry) {
                return Promise.reject(error);
            }
            if (isRefreshing) {
                return Promise.reject(error);
            }
            originalRequest._retry = true;
            try {
                isRefreshing = true;
                const dispatch: AppDispatch = store.dispatch;
                await refreshAccessToken(dispatch);
                const newAccessToken = store.getState().auth.accessToken;
                if (newAccessToken) {
                    originalRequest.headers = {
                        ...(originalRequest.headers || {}),
                        Authorization: `Bearer ${newAccessToken}`
                    };
                }
                isRefreshing = false;
                return axios(originalRequest);
            } catch {
                isRefreshing = false;
                window.location.href = "/login";
                return Promise.reject(error);
            }
        }
        return Promise.reject(error);
    }
);
