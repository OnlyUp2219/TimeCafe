import {afterEach, beforeEach, describe, expect, it, vi} from "vitest";
import {renderHook} from "@testing-library/react";

vi.mock("@api/apiBaseUrl", () => ({
    getApiBaseUrl: vi.fn(() => "https://api.local"),
}));

import {useExternalAuthLogin} from "@hooks/useExternalAuthLogin";

describe("useExternalAuthLogin", () => {
    beforeEach(() => {
        vi.stubGlobal(
            "window",
            {
                location: {
                    origin: "https://app.local",
                    href: "",
                },
            } as Window
        );
    });

    afterEach(() => {
        vi.unstubAllGlobals();
    });

    it("builds google auth URL", () => {
        const {result} = renderHook(() => useExternalAuthLogin());

        result.current.handleGoogleLogin();

        expect(window.location.href).toBe(
            "https://api.local/auth/authenticate/login/google?returnUrl=https%3A%2F%2Fapp.local%2Fexternal-callback"
        );
    });

    it("builds microsoft auth URL", () => {
        const {result} = renderHook(() => useExternalAuthLogin());

        result.current.handleMicrosoftLogin();

        expect(window.location.href).toBe(
            "https://api.local/auth/authenticate/login/microsoft?returnUrl=https%3A%2F%2Fapp.local%2Fexternal-callback"
        );
    });
});
