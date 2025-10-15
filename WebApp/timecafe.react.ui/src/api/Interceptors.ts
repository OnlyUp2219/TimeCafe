import axios from "axios";
import {refreshToken} from "./auth.ts";

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
                await refreshToken();
                isRefreshing = false;

                originalRequest.headers["Authorization"] =
                    `Bearer ${localStorage.getItem("accessToken")}`;
                return axios(originalRequest);
            } catch {
                isRefreshing = false;
                window.location.href = "/login";
            }
        }

        return Promise.reject(error);
    }
);
