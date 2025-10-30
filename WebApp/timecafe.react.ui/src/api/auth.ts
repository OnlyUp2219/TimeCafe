import axios from "axios";
import {clearTokens, setAccessToken, setRefreshToken} from "../store/authSlice.ts";
import type {AppDispatch} from "../store";
import {store} from "../store";

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface ResetPasswordEmailRequest {
    email: string;
}

export interface ResetPasswordRequest {
    email: string;
    resetCode: string;
    newPassword: string;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
}

export interface PhoneCodeRequest {
    phoneNumber: string;
    code: string;
    captchaToken?: string;
}

export interface VerifyPhoneResponse {
    message: string;
    remainingAttempts?: number;
    requiresCaptcha?: boolean;
}

const apiBase = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7057";
const USE_MOCK_SMS = import.meta.env.VITE_USE_MOCK_SMS === "true";
const USE_MOCK_EMAIL = import.meta.env.VITE_USE_MOCK_EMAIL === "true";

export async function registerUser(data: RegisterRequest, dispatch: AppDispatch): Promise<void> {
    try {
        const res = await axios.post(`${apiBase}/registerWithUsername`, data, {
            headers: {"Content-Type": "application/json"},
        });

        const tokens = res.data as { accessToken: string; refreshToken: string };
        dispatch(setAccessToken(tokens.accessToken));
        dispatch(setRefreshToken(tokens.refreshToken));
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;

            if (res?.data) {
                throw res?.data;
            }

            throw new Error(`Ошибка регистрации (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при регистрации");
    }
}

export async function loginUser(data: LoginRequest, dispatch: AppDispatch): Promise<void> {
    try {
        const res = await axios.post(`${apiBase}/login-jwt`, data, {
            headers: {"Content-Type": "application/json"},
        });

        const tokens = res.data as { accessToken: string; refreshToken: string };
        dispatch(setAccessToken(tokens.accessToken));
        dispatch(setRefreshToken(tokens.refreshToken));
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;

            if (res?.data) {
                throw res?.data;
            }

            throw new Error(`Ошибка входа (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при входе");
    }
}

export async function refreshToken(refreshToken: string, dispatch: AppDispatch): Promise<void> {
    if (!refreshToken) throw new Error("Нет refresh токена");

    try {
        const res = await axios.post(`${apiBase}/refresh-token-jwt`, {refreshToken}, {
            headers: {"Content-Type": "application/json"},
        });

        const tokens = res.data as { accessToken: string; refreshToken: string };
        dispatch(setAccessToken(tokens.accessToken));
        dispatch(setRefreshToken(tokens.refreshToken));
    } catch {
        dispatch(clearTokens());
        throw new Error("Не удалось обновить токен");
    }
}

export async function forgotPassword(data: ResetPasswordEmailRequest): Promise<{
    message?: string;
    callbackUrl?: string
}> {
    const endpoint = USE_MOCK_EMAIL ? "/forgot-password-link-mock" : "/forgot-password-link";
    console.log("Using endpoint:", endpoint);
    console.log(data);
    console.log(Date.now());
    try {
        const response = await axios.post<{ message?: string; callbackUrl?: string }>(`${apiBase}${endpoint}`, data, {
            headers: {"Content-Type": "application/json"},
        });

        return response.data;
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;
            if (res?.data) {
                throw res?.data;
            }
            throw new Error(`Ошибка отправки (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при попытке отправить сообщение на почту");
    }
}

export async function resetPassword(data: ResetPasswordRequest): Promise<void> {
    try {
        await axios.post(`${apiBase}/resetPassword`, data, {
            headers: {"Content-Type": "application/json"},
        });
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;
            if (res?.data) {
                throw res?.data;
            }
            throw new Error(`Ошибка отправки (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при попытке отправить сообщение на почту");
    }
}

export async function changePassword(data: ChangePasswordRequest): Promise<void> {
    const state = store.getState();
    const accessToken = state.auth?.accessToken;
    try {
        await axios.post(`${apiBase}/account/change-password`, data, {
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${accessToken}`
            }
        });
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;
            if (res?.data) {
                throw res.data;
            }
            throw new Error(`Ошибка смены пароля (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при смене пароля");
    }
}

export async function logoutServer(refreshToken: string | null, dispatch: AppDispatch): Promise<void> {
    try {
        if (refreshToken) {
            await axios.post(`${apiBase}/logout`, {refreshToken}, {
                headers: {"Content-Type": "application/json"}
            });
        }
    } catch (e) {
    } finally {
        dispatch(clearTokens());
    }
}

export async function SendPhoneConfirmation(data: PhoneCodeRequest): Promise<void> {
    const state = store.getState();
    const accessToken = state.auth?.accessToken;
    const endpoint = USE_MOCK_SMS ? "/twilio/generateSMS-mock" : "/twilio/generateSMS";

    try {
        await axios.post(`${apiBase}${endpoint}`, data, {
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${accessToken}`
            }
        })
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;
            if (res?.data) {
                throw res?.data;
            }

            throw new Error(`Ошибка отправки кода на телефон - (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при отправке смс");
    }
}

export async function VerifyPhoneConfirmation(data: PhoneCodeRequest): Promise<VerifyPhoneResponse> {
    const state = store.getState();
    const accessToken = state.auth?.accessToken;
    const endpoint = USE_MOCK_SMS ? "/twilio/verifySMS-mock" : "/twilio/verifySMS";

    try {
        const response = await axios.post(`${apiBase}${endpoint}`, data, {
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${accessToken}`
            }
        });
        return response.data;
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;
            if (res?.data) {
                throw res?.data;
            }
            throw new Error(`Ошибка подтверждение кода - (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при подтверждение кода");
    }
}