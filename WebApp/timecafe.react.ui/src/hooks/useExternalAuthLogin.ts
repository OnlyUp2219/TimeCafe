import {useCallback} from "react";
import {getApiBaseUrl} from "@api/apiBaseUrl";

export const useExternalAuthLogin = () => {
    const apiBase = getApiBaseUrl();
    const returnUrl = `${globalThis.location.origin}/external-callback`;

    const loginWithProvider = useCallback((provider: "google" | "microsoft") => {
        globalThis.location.href = `${apiBase}/auth/authenticate/login/${provider}?returnUrl=${encodeURIComponent(returnUrl)}`;
    }, [apiBase, returnUrl]);

    const handleGoogleLogin = useCallback(() => loginWithProvider("google"), [loginWithProvider]);
    const handleMicrosoftLogin = useCallback(() => loginWithProvider("microsoft"), [loginWithProvider]);

    return {handleGoogleLogin, handleMicrosoftLogin};
};
