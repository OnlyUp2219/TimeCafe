import {createApi} from "@reduxjs/toolkit/query/react";
import {baseQueryWithReauth} from "@store/api/baseQuery";

const USE_MOCK_SMS = import.meta.env.VITE_USE_MOCK_SMS === "true";
const USE_MOCK_EMAIL = import.meta.env.VITE_USE_MOCK_EMAIL === "true";

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

export interface PhoneVerificationStatusResponse {
    phoneNumber: string | null;
    phoneNumberConfirmed: boolean;
    hasPendingVerification: boolean;
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

export const authApi = createApi({
    reducerPath: "authApi",
    baseQuery: baseQueryWithReauth,
    endpoints: (builder) => ({
        loginJwtV2: builder.mutation<LoginJwtV2Response, LoginRequest>({
            query: (data) => ({
                url: "/auth/login-jwt-v2",
                method: "POST",
                body: data,
            }),
        }),

        register: builder.mutation<{ callbackUrl?: string }, RegisterRequest>({
            query: (data) => ({
                url: `/auth/${USE_MOCK_EMAIL ? "registerWithUsername-mock" : "registerWithUsername"}`,
                method: "POST",
                body: data,
            }),
        }),

        logout: builder.mutation<void, void>({
            query: () => ({
                url: "/auth/logout",
                method: "POST",
            }),
        }),

        getCurrentUser: builder.query<CurrentUserResponse, void>({
            query: () => "/auth/account/me",
        }),

        getPhoneVerificationStatus: builder.query<PhoneVerificationStatusResponse, void>({
            query: () => "/auth/account/phone-verification-status",
        }),

        forgotPasswordLink: builder.mutation<{ message?: string; callbackUrl?: string }, ResetPasswordEmailRequest>({
            query: (data) => ({
                url: `/auth/${USE_MOCK_EMAIL ? "forgot-password-link-mock" : "forgot-password-link"}`,
                method: "POST",
                body: data,
            }),
        }),

        resetPassword: builder.mutation<void, ResetPasswordRequest>({
            query: (data) => ({
                url: "/auth/resetPassword",
                method: "POST",
                body: data,
            }),
        }),

        changePassword: builder.mutation<void, ChangePasswordRequest>({
            query: (data) => ({
                url: "/auth/account/change-password",
                method: "POST",
                body: data,
            }),
        }),

        savePhoneNumber: builder.mutation<void, SavePhoneRequest>({
            query: (data) => ({
                url: "/auth/account/phone",
                method: "POST",
                body: data,
            }),
        }),

        clearPhoneNumber: builder.mutation<void, void>({
            query: () => ({
                url: "/auth/account/phone",
                method: "DELETE",
            }),
        }),

        sendPhoneConfirmation: builder.mutation<SendPhoneResponse, PhoneCodeRequest>({
            query: (data) => ({
                url: `/auth/${USE_MOCK_SMS ? "twilio/generateSMS-mock" : "twilio/generateSMS"}`,
                method: "POST",
                body: data,
            }),
        }),

        verifyPhoneConfirmation: builder.mutation<VerifyPhoneResponse, PhoneCodeRequest>({
            query: (data) => ({
                url: `/auth/${USE_MOCK_SMS ? "twilio/verifySMS-mock" : "twilio/verifySMS"}`,
                method: "POST",
                body: data,
            }),
        }),

        resendConfirmation: builder.mutation<{ message?: string; callbackUrl?: string }, { email: string }>({
            query: (data) => ({
                url: `/auth/${USE_MOCK_EMAIL ? "email/resend-mock" : "email/resend"}`,
                method: "POST",
                body: data,
            }),
        }),

        confirmEmail: builder.mutation<{ message?: string; error?: string }, { userId: string; token: string }>({
            query: (data) => ({
                url: "/auth/email/confirm",
                method: "POST",
                body: data,
            }),
        }),

        requestEmailChange: builder.mutation<ChangeEmailResponse, ChangeEmailRequest>({
            query: (data) => ({
                url: `/auth/${USE_MOCK_EMAIL ? "email/change-mock" : "email/change"}`,
                method: "POST",
                body: data,
            }),
        }),

        confirmEmailChange: builder.mutation<
            { message?: string; error?: string },
            { userId: string; newEmail: string; token: string }
        >({
            query: (data) => ({
                url: "/auth/email/change-confirm",
                method: "POST",
                body: data,
            }),
        }),
    }),
});

export const {
    useLoginJwtV2Mutation,
    useRegisterMutation,
    useLogoutMutation,
    useGetCurrentUserQuery,
    useLazyGetCurrentUserQuery,
    useLazyGetPhoneVerificationStatusQuery,
    useForgotPasswordLinkMutation,
    useResetPasswordMutation,
    useChangePasswordMutation,
    useSavePhoneNumberMutation,
    useClearPhoneNumberMutation,
    useSendPhoneConfirmationMutation,
    useVerifyPhoneConfirmationMutation,
    useResendConfirmationMutation,
    useConfirmEmailMutation,
    useRequestEmailChangeMutation,
    useConfirmEmailChangeMutation,
} = authApi;
