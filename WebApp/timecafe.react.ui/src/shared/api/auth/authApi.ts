import {httpClient, rawHttpClient} from "@api/httpClient";
import {normalizeUnknownError} from "@api/errors/normalize";
import type {RateLimitedResponse} from "@utility/rateLimitHelper.ts";
import {withRateLimit} from "@utility/rateLimitHelper.ts";

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

export interface ChangeEmailRequest {
    newEmail: string;
}

export interface SavePhoneRequest {
    phoneNumber: string;
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

export interface SendPhoneResponse {
    message?: string;
    phoneNumber?: string;
    token?: string;
}

export interface ChangeEmailResponse {
    message?: string;
    callbackUrl?: string;
}

export interface LoginJwtV2Response {
    accessToken: string;
    refreshToken?: string;
    role?: string;
    expiresIn?: number;
    emailConfirmed?: boolean;
}

export interface CurrentUserResponse {
    userId: string;
    email: string;
    emailConfirmed: boolean;
    phoneNumber?: string | null;
    phoneNumberConfirmed: boolean;
}

const USE_MOCK_SMS = import.meta.env.VITE_USE_MOCK_SMS === "true";
const USE_MOCK_EMAIL = import.meta.env.VITE_USE_MOCK_EMAIL === "true";

const AUTH_PREFIX = "/auth";

let refreshAccessTokenInFlight: Promise<string | null> | null = null;

const loginJwtV2 = async (data: LoginRequest): Promise<LoginJwtV2Response> => {
    try {
        const res = await httpClient.post<LoginJwtV2Response>(`${AUTH_PREFIX}/login-jwt-v2`, data);
        return res.data;
    } catch (e) {
        throw normalizeUnknownError(e);
    }
};

const registerWithUsername = async (data: RegisterRequest): Promise<{ callbackUrl?: string }> => {
    try {
        const endpoint = USE_MOCK_EMAIL ? "/registerWithUsername-mock" : "/registerWithUsername";
        const res = await httpClient.post<{ callbackUrl?: string }>(`${AUTH_PREFIX}${endpoint}`, data);
        return res.data;
    } catch (e) {
        throw normalizeUnknownError(e);
    }
};

const tryRefreshAccessToken = async (): Promise<string | null> => {
    refreshAccessTokenInFlight ??= (async () => {
        try {
            const res = await rawHttpClient.post<{ accessToken: string }>(`${AUTH_PREFIX}/refresh-jwt-v2`, {}, {
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

const logout = async (): Promise<void> => {
    try {
        await rawHttpClient.post(`${AUTH_PREFIX}/logout`, null, {
            _manualRefresh: true,
        });
    } catch {
        void 0;
    }
};

const forgotPasswordLink = async (data: ResetPasswordEmailRequest): Promise<RateLimitedResponse<{ message?: string; callbackUrl?: string }>> => {
    const endpoint = USE_MOCK_EMAIL ? "/forgot-password-link-mock" : "/forgot-password-link";
    return withRateLimit(() => httpClient.post<{ message?: string; callbackUrl?: string }>(`${AUTH_PREFIX}${endpoint}`, data));
};

const resetPassword = async (data: ResetPasswordRequest): Promise<void> => {
    try {
        await httpClient.post(`${AUTH_PREFIX}/resetPassword`, data);
    } catch (e) {
        throw normalizeUnknownError(e);
    }
};

const changePassword = async (data: ChangePasswordRequest): Promise<void> => {
    try {
        await httpClient.post(`${AUTH_PREFIX}/account/change-password`, data);
    } catch (e) {
        throw normalizeUnknownError(e);
    }
};

const savePhoneNumber = async (data: SavePhoneRequest): Promise<void> => {
    try {
        await httpClient.post(`${AUTH_PREFIX}/account/phone`, data);
    } catch (e) {
        throw normalizeUnknownError(e);
    }
};

const clearPhoneNumber = async (): Promise<void> => {
    try {
        await httpClient.delete(`${AUTH_PREFIX}/account/phone`);
    } catch (e) {
        throw normalizeUnknownError(e);
    }
};

const sendPhoneConfirmation = async (data: PhoneCodeRequest): Promise<RateLimitedResponse<SendPhoneResponse>> => {
    const endpoint = USE_MOCK_SMS ? "/twilio/generateSMS-mock" : "/twilio/generateSMS";
    return withRateLimit(() => httpClient.post<SendPhoneResponse>(`${AUTH_PREFIX}${endpoint}`, data));
};

const verifyPhoneConfirmation = async (data: PhoneCodeRequest): Promise<VerifyPhoneResponse> => {
    const endpoint = USE_MOCK_SMS ? "/twilio/verifySMS-mock" : "/twilio/verifySMS";
    try {
        const res = await httpClient.post<VerifyPhoneResponse>(`${AUTH_PREFIX}${endpoint}`, data);
        return res.data;
    } catch (e) {
        throw normalizeUnknownError(e);
    }
};

const resendConfirmation = async (email: string): Promise<RateLimitedResponse<{ message?: string; callbackUrl?: string }>> => {
    const endpoint = USE_MOCK_EMAIL ? "/email/resend-mock" : "/email/resend";
    return withRateLimit(() => httpClient.post(`${AUTH_PREFIX}${endpoint}`, {email}));
};

const confirmEmail = async (userId: string, token: string): Promise<{ message?: string; error?: string }> => {
    try {
        const res = await httpClient.post<{ message?: string; error?: string }>(`${AUTH_PREFIX}/email/confirm`, {userId, token});
        return res.data;
    } catch (e) {
        const err = normalizeUnknownError(e);
        return {error: err.message};
    }
};

const requestEmailChange = async (data: ChangeEmailRequest): Promise<RateLimitedResponse<ChangeEmailResponse>> => {
    const endpoint = USE_MOCK_EMAIL ? "/email/change-mock" : "/email/change";
    return withRateLimit(() => httpClient.post(`${AUTH_PREFIX}${endpoint}`, data));
};

const confirmEmailChange = async (userId: string, newEmail: string, token: string): Promise<{ message?: string; error?: string }> => {
    try {
        const res = await httpClient.post<{ message?: string; error?: string }>(`${AUTH_PREFIX}/email/change-confirm`, {
            userId,
            newEmail,
            token,
        });
        return res.data;
    } catch (e) {
        const err = normalizeUnknownError(e);
        return {error: err.message};
    }
};

const getCurrentUser = async (): Promise<CurrentUserResponse> => {
    try {
        const res = await httpClient.get<CurrentUserResponse>(`${AUTH_PREFIX}/account/me`);
        return res.data;
    } catch (e) {
        throw normalizeUnknownError(e);
    }
};

export const authApi = {
    loginJwtV2,
    registerWithUsername,
    tryRefreshAccessToken,
    logout,
    forgotPasswordLink,
    resetPassword,
    changePassword,
    savePhoneNumber,
    clearPhoneNumber,
    sendPhoneConfirmation,
    verifyPhoneConfirmation,
    resendConfirmation,
    confirmEmail,
    requestEmailChange,
    confirmEmailChange,
    getCurrentUser,
};
