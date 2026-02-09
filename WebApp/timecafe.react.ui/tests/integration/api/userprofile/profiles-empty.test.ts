import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, loginAndGetAccessToken, registerAndConfirm, parseCallbackParams, registerUser} from "../helpers";

describe("/userprofile/profiles/empty", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.post("/userprofile/profiles/empty/00000000-0000-0000-0000-000000000001", null);
        expect(res.status).toBe(401);
    });

    it("creates empty profile for user", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        const callbackUrl = registerRes.data?.callbackUrl as string | undefined;
        let userId = "";
        if (callbackUrl) {
            const parsed = parseCallbackParams(callbackUrl);
            userId = parsed.userId;
            if (parsed.userId && parsed.token) {
                await client.post("/auth/email/confirm", {userId: parsed.userId, token: parsed.token});
            }
        }

        const {token} = await loginAndGetAccessToken(client, email, password);
        expect(token).toBeTruthy();

        if (userId) {
            const res = await client.post(`/userprofile/profiles/empty/${userId}`, null, {headers: {Authorization: `Bearer ${token}`}});
            if (res.status !== 409) {
                expect(res.status).toBeGreaterThanOrEqual(200);
                expect(res.status).toBeLessThan(300);
            }
        }
    });

    it("returns 409 when profile already exists", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        const callbackUrl = registerRes.data?.callbackUrl as string | undefined;
        let userId = "";
        if (callbackUrl) {
            const parsed = parseCallbackParams(callbackUrl);
            userId = parsed.userId;
            if (parsed.userId && parsed.token) {
                await client.post("/auth/email/confirm", {userId: parsed.userId, token: parsed.token});
            }
        }

        const {token} = await loginAndGetAccessToken(client, email, password);
        expect(token).toBeTruthy();

        if (userId) {
            await client.post(`/userprofile/profiles/empty/${userId}`, null, {headers: {Authorization: `Bearer ${token}`}});
            const res = await client.post(`/userprofile/profiles/empty/${userId}`, null, {headers: {Authorization: `Bearer ${token}`}});
            expect(res.status).toBe(409);
        }
    });

    it("returns 422 for invalid userId", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        const callbackUrl = registerRes.data?.callbackUrl as string | undefined;
        if (callbackUrl) {
            const parsed = parseCallbackParams(callbackUrl);
            if (parsed.userId && parsed.token) {
                await client.post("/auth/email/confirm", {userId: parsed.userId, token: parsed.token});
            }
        }

        const {token} = await loginAndGetAccessToken(client, email, password);
        const res = await client.post("/userprofile/profiles/empty/invalid", null, {headers: {Authorization: `Bearer ${token}`}});
        expect(res.status).toBe(422);
    });
});
