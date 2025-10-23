import axios from "axios";
import {refreshToken as refreshTokenApi} from "./auth.ts";
import {store} from "../store";
import type {AppDispatch} from "../store";

let isRefreshing = false;

axios.interceptors.response.use(
    response => response,
    async error => {
        const originalRequest = error.config;

        if (error.response?.status === 401 && !originalRequest._retry) {
            if (isRefreshing) {
                return Promise.reject(error);
            }

            originalRequest._retry = true;

            try {
                isRefreshing = true;
                const state = store.getState();
                const refreshToken = state.auth.refreshToken;
                const dispatch: AppDispatch = store.dispatch;

                await refreshTokenApi(refreshToken, dispatch);

                const updatedState = store.getState();
                const newAccessToken = updatedState.auth.accessToken;

                originalRequest.headers["Authorization"] = `Bearer ${newAccessToken}`;

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