import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, registerUser, parseCallbackParams, registerAndConfirm} from "@tests/integration/api/helpers";

describe("/auth/email/confirm", () => {
    it("confirms email with valid token", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        expect(registerRes.status).toBeGreaterThanOrEqual(200);
        expect(registerRes.status).toBeLessThan(300);
        const callbackUrl = registerRes.data?.callbackUrl as string | undefined;
        expect(callbackUrl).toBeTruthy();

        if (callbackUrl) {
            const {userId, token} = parseCallbackParams(callbackUrl);
            const res = await client.post("/auth/email/confirm", {userId, token});
            expect(res.status).toBeGreaterThanOrEqual(200);
            expect(res.status).toBeLessThan(300);
        }
    });

    it("returns 400 for invalid token", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        const callbackUrl = registerRes.data?.callbackUrl as string | undefined;
        expect(callbackUrl).toBeTruthy();

        if (callbackUrl) {
            const {userId} = parseCallbackParams(callbackUrl);
            const res = await client.post("/auth/email/confirm", {
                userId,
                token: "invalid",
            });
            expect(res.status).toBe(400);
        }
    });

    it("returns 422 for missing userId", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/email/confirm", {userId: "", token: "x"});
        expect(res.status).toBe(422);
    });

    it("returns 401 for user not found", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/email/confirm", {
            userId: "00000000-0000-0000-0000-000000000111",
            token: "dummy-token",
        });
        expect(res.status).toBe(401);
    });

    it("returns 400 for already confirmed user", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const {userId} = await registerAndConfirm(client, email, password);
        expect(userId).toBeTruthy();

        if (userId) {
            const res = await client.post("/auth/email/confirm", {userId, token: "dummy-token"});
            expect(res.status).toBe(400);
        }
    });

    it("returns 400 for invalid token format", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        const callbackUrl = registerRes.data?.callbackUrl as string | undefined;
        expect(callbackUrl).toBeTruthy();

        if (callbackUrl) {
            const {userId} = parseCallbackParams(callbackUrl);
            const res = await client.post("/auth/email/confirm", {
                userId,
                token: "!!!@@@",
            });
            expect(res.status).toBe(400);
        }
    });
});
