import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, loginAndGetAccessToken, registerAndConfirm, withAuthHeader} from "../helpers";

describe("/auth/account/change-password", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/account/change-password", {
            currentPassword: "old",
            newPassword: "new",
        });
        expect(res.status).toBe(401);
    });

    it("returns 400 when current password is wrong", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {token} = await loginAndGetAccessToken(client, email, password);

        const res = await client.post(
            "/auth/account/change-password",
            {currentPassword: "wrong", newPassword: "Test1234!3"},
            withAuthHeader(client, token)
        );
        expect(res.status).toBe(400);
    });

    it("changes password successfully", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();
        const nextPassword = "Test1234!3";

        await registerAndConfirm(client, email, password);
        const {token} = await loginAndGetAccessToken(client, email, password);

        const res = await client.post(
            "/auth/account/change-password",
            {currentPassword: password, newPassword: nextPassword},
            withAuthHeader(client, token)
        );
        expect(res.status).toBeGreaterThanOrEqual(200);
        expect(res.status).toBeLessThan(300);

        const loginRes = await client.post("/auth/login-jwt-v2", {email, password: nextPassword});
        expect(loginRes.status).toBe(200);
    });
});
