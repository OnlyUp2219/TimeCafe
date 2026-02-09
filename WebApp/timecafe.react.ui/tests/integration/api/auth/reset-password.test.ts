import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, registerAndConfirm, parseCallbackParams} from "../helpers";

describe("/auth/resetPassword", () => {
    it("resets password and allows login", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();
        const newPassword = "Test1234!2";

        await registerAndConfirm(client, email, password);

        const forgotRes = await client.post("/auth/forgot-password-link-mock", {email});
        expect(forgotRes.status).toBe(200);
        const resetUrl = forgotRes.data?.callbackUrl as string | undefined;
        expect(resetUrl).toBeTruthy();

        if (resetUrl) {
            const {email: resetEmail, code} = parseCallbackParams(resetUrl);
            const resetRes = await client.post("/auth/resetPassword", {
                email: resetEmail,
                resetCode: code,
                newPassword,
            });
            expect(resetRes.status).toBeGreaterThanOrEqual(200);
            expect(resetRes.status).toBeLessThan(300);
        }

        const loginRes = await client.post("/auth/login-jwt-v2", {email, password: newPassword});
        expect(loginRes.status).toBe(200);
    });

    it("returns 422 when code missing", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/resetPassword", {
            email: "user@example.com",
            resetCode: "",
            newPassword: "Test1234!2",
        });
        expect(res.status).toBe(422);
    });
});
