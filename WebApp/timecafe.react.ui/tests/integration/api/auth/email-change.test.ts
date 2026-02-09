import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, loginAndGetAccessToken, registerAndConfirm, parseCallbackParams, withAuthHeader} from "../helpers";

describe("/auth/email/change", () => {
    it("changes email with confirm flow", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();
        const newEmail = createTestEmail();

        await registerAndConfirm(client, email, password);
        const {token} = await loginAndGetAccessToken(client, email, password);
        expect(token).toBeTruthy();

        const changeRes = await client.post(
            "/auth/email/change-mock",
            {newEmail},
            withAuthHeader(client, token)
        );
        expect(changeRes.status).toBe(200);
        const changeUrl = changeRes.data?.callbackUrl as string | undefined;
        expect(changeUrl).toBeTruthy();

        if (changeUrl) {
            const {userId, token: confirmToken, newEmail: parsedEmail} = parseCallbackParams(changeUrl);
            const confirmRes = await client.post("/auth/email/change-confirm", {
                userId,
                newEmail: parsedEmail,
                token: confirmToken,
            });
            expect(confirmRes.status).toBeGreaterThanOrEqual(200);
            expect(confirmRes.status).toBeLessThan(300);
        }

        const loginRes = await client.post("/auth/login-jwt-v2", {email: newEmail, password});
        expect(loginRes.status).toBe(200);
    });

    it("returns 400 for invalid token", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();
        const newEmail = createTestEmail();

        const {userId} = await registerAndConfirm(client, email, password);
        expect(userId).toBeTruthy();

        if (userId) {
            const res = await client.post("/auth/email/change-confirm", {
                userId,
                newEmail,
                token: "invalid",
            });
            expect(res.status).toBe(400);
        }
    });

    it("returns 401 for user not found", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/email/change-confirm", {
            userId: "00000000-0000-0000-0000-000000000222",
            newEmail: createTestEmail(),
            token: "dummy-token",
        });
        expect(res.status).toBe(401);
    });

    it("returns 400 for invalid token format", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();
        const newEmail = createTestEmail();

        const {userId} = await registerAndConfirm(client, email, password);
        expect(userId).toBeTruthy();

        if (userId) {
            const res = await client.post("/auth/email/change-confirm", {
                userId,
                newEmail,
                token: "!!!@@@",
            });
            expect(res.status).toBe(400);
        }
    });

    it("returns 422 for invalid newEmail", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const {userId} = await registerAndConfirm(client, email, password);
        expect(userId).toBeTruthy();

        if (userId) {
            const res = await client.post("/auth/email/change-confirm", {
                userId,
                newEmail: "",
                token: "x",
            });
            expect(res.status).toBe(422);
        }
    });
});
