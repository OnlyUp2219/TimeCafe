import axios from "axios";
import {clearTokens, setAccessToken, setEmail, setEmailConfirmed} from "../../store/authSlice.ts";
import type {AppDispatch} from "../../store";
import {store} from "../../store";
import {withRateLimit, type RateLimitedResponse} from "../../utility/rateLimitHelper.ts";

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

const apiBase = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7268";
const USE_MOCK_SMS = import.meta.env.VITE_USE_MOCK_SMS === "true";
const USE_MOCK_EMAIL = import.meta.env.VITE_USE_MOCK_EMAIL === "true";

export async function registerUser(data: RegisterRequest, dispatch: AppDispatch): Promise<{ callbackUrl?: string }> {
    try {
        const endpoint = USE_MOCK_EMAIL ? "/registerWithUsername-mock" : "/registerWithUsername";
        const res = await axios.post(`${apiBase}/auth${endpoint}`, data, {
            headers: {"Content-Type": "application/json"},
        });
        dispatch(setEmail(data.email));
        dispatch(setEmailConfirmed(false));
        if (res.data.callbackUrl) return {callbackUrl: res.data.callbackUrl};
        return {};
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;
            if (res?.data) throw res.data;
            throw new Error(`Ошибка регистрации (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при регистрации");
    }
}

export async function loginUser(data: LoginRequest, dispatch: AppDispatch): Promise<{ emailNotConfirmed?: boolean }> {
    try {
        const res = await axios.post(`${apiBase}/auth/login-jwt-v2`, data, {
            headers: {"Content-Type": "application/json"},
            withCredentials: true
        });

        if (res.data.emailConfirmed === false) {
            dispatch(setEmailConfirmed(false));
            return {emailNotConfirmed: true};
        }
        dispatch(setEmail(data.email));


        const tokens = res.data as { accessToken: string };
        dispatch(setAccessToken(tokens.accessToken));
        dispatch(setEmail(data.email));
        dispatch(setEmailConfirmed(typeof res.data.emailConfirmed === "boolean" ? res.data.emailConfirmed : false));
        return {};
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;
            if (res?.data) {
                throw res.data;
            }
            throw new Error(`Ошибка входа (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при входе");
    }
}

export async function refreshAccessToken(dispatch: AppDispatch): Promise<void> {
    try {
        console.log("[refreshAccessToken] document.cookie перед запросом:", document.cookie);
        const res = await axios.post(`${apiBase}/auth/refresh-jwt-v2`, {}, {
            headers: {"Content-Type": "application/json"},
            withCredentials: true
        });
        console.log("[refreshAccessToken] status:", res.status);
        const tokens = res.data as { accessToken: string };
        dispatch(setAccessToken(tokens.accessToken));
        console.log("[refreshAccessToken] новый accessToken получен (длина)", tokens.accessToken.length);
    } catch (e) {
        if (axios.isAxiosError(e)) {
            const r = e.response;
            console.warn("[refreshAccessToken] ошибка axios", r?.status, r?.data, "cookie сейчас:", document.cookie);
        } else {
            console.warn("[refreshAccessToken] неизвестная ошибка", e, "cookie сейчас:", document.cookie);
        }
        throw new Error("Не удалось обновить токен (см. консоль браузера)");
    }
}

export async function forgotPassword(data: ResetPasswordEmailRequest): Promise<RateLimitedResponse<{
    message?: string;
    callbackUrl?: string;
}>> {
    const endpoint = USE_MOCK_EMAIL ? "/forgot-password-link-mock" : "/forgot-password-link";

    return withRateLimit(() =>
        axios.post<{ message?: string; callbackUrl?: string }>(`${apiBase}/auth${endpoint}`, data, {
            headers: {"Content-Type": "application/json"},
        })
    );
}

export async function resetPassword(data: ResetPasswordRequest): Promise<void> {
    try {
        await axios.post(`${apiBase}/auth/resetPassword`, data, {
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
        await axios.post(`${apiBase}/auth/account/change-password`, data, {
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

export async function logoutServer(dispatch: AppDispatch): Promise<void> {
    try {
        await axios.post(`${apiBase}/auth/logout`, null, { withCredentials: true });
    } catch {
        void 0;
    } finally {
        dispatch(clearTokens());
    }
}

export async function SendPhoneConfirmation(data: PhoneCodeRequest): Promise<RateLimitedResponse<void>> {
    const state = store.getState();
    const accessToken = state.auth?.accessToken;
    const endpoint = USE_MOCK_SMS ? "/twilio/generateSMS-mock" : "/twilio/generateSMS";

    return withRateLimit(() =>
        axios.post<void>(`${apiBase}/auth${endpoint}`, data, {
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${accessToken}`
            }
        })
    );
}

export async function VerifyPhoneConfirmation(data: PhoneCodeRequest): Promise<VerifyPhoneResponse> {
    const state = store.getState();
    const accessToken = state.auth?.accessToken;
    const endpoint = USE_MOCK_SMS ? "/twilio/verifySMS-mock" : "/twilio/verifySMS";
    try {
        const response = await axios.post(`${apiBase}/auth${endpoint}`, data, {
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${accessToken}`
            }
        });
        return response.data;
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const res = error.response;
            if (res?.data) throw res?.data;
            throw new Error(`Ошибка подтверждение кода - (${res?.status ?? "нет ответа"})`);
        }
        throw new Error("Неизвестная ошибка при подтверждение кода");
    }
}

export async function resendConfirmation(email: string): Promise<RateLimitedResponse<{
    message?: string;
    callbackUrl?: string
}>> {
    const endpoint = USE_MOCK_EMAIL ? "/email/resend-mock" : "/email/resend";
    return withRateLimit(() => axios.post(`${apiBase}/auth${endpoint}`, {email}, {headers: {"Content-Type": "application/json"}}));
}

export async function confirmEmail(userId: string, token: string): Promise<{ message?: string; error?: string }> {
    try {
        const res = await axios.post(`${apiBase}/auth/email/confirm`, {
            userId,
            token
        }, {headers: {"Content-Type": "application/json"}});
        return res.data;
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const r = error.response;
            if (r?.data) return r.data;
            return {error: `Ошибка подтверждения (${r?.status ?? "нет ответа"})`};
        }
        return {error: "Неизвестная ошибка подтверждения"};
    }
}
