import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, loginAndGetAccessToken, registerAndConfirm, withAuthHeader} from "../helpers";

describe("/auth/account/me", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.get("/auth/account/me");
        expect(res.status).toBe(401);
    });

    it("returns current user when authorized", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {token} = await loginAndGetAccessToken(client, email, password);
        expect(token).toBeTruthy();

        const res = await client.get("/auth/account/me", withAuthHeader(client, token));
        expect(res.status).toBe(200);
        expect(res.data?.email).toBe(email);
    });
});
