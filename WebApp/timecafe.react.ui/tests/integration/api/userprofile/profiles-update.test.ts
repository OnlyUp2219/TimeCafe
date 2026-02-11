import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, loginAndGetAccessToken, parseCallbackParams, registerUser} from "@tests/integration/api/helpers";

describe("/userprofile/profiles (update)", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.put("/userprofile/profiles", {userId: "id", firstName: "A", lastName: "B", gender: 0});
        expect(res.status).toBe(401);
    });

    it("updates profile when authorized", async () => {
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
            const res = await client.put(
                "/userprofile/profiles",
                {userId, firstName: "Test", lastName: "User", gender: 0},
                {headers: {Authorization: `Bearer ${token}`}}
            );
            expect(res.status).toBeGreaterThanOrEqual(200);
            expect(res.status).toBeLessThan(300);
        }
    });

    it("returns 422 for invalid payload", async () => {
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
        const res = await client.put(
            "/userprofile/profiles",
            {userId: "00000000-0000-0000-0000-000000000123", firstName: "", lastName: "", gender: 0},
            {headers: {Authorization: `Bearer ${token}`}}
        );
        expect(res.status).toBe(422);
    });

    it("returns 404 when profile missing", async () => {
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
        const res = await client.put(
            "/userprofile/profiles",
            {userId: "00000000-0000-0000-0000-000000000999", firstName: "Test", lastName: "User", gender: 0},
            {headers: {Authorization: `Bearer ${token}`}}
        );
        expect(res.status).toBe(404);
    });
});
