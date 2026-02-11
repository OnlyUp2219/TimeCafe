import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, loginAndGetAccessToken, registerAndConfirm, withAuthHeader} from "@tests/integration/api/helpers";

describe("/auth/account/phone (clear)", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.delete("/auth/account/phone");
        expect(res.status).toBe(401);
    });

    it("clears phone when authorized", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {token} = await loginAndGetAccessToken(client, email, password);

        const res = await client.delete("/auth/account/phone", withAuthHeader(client, token));
        expect(res.status).toBe(200);
    });
});
