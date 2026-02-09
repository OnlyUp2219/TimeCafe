import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, loginAndGetAccessToken, registerAndConfirm, withAuthHeader} from "../helpers";

describe("/auth/account/phone", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/account/phone", {phoneNumber: "+79123456789"});
        expect(res.status).toBe(401);
    });

    it("saves phone when authorized", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {token} = await loginAndGetAccessToken(client, email, password);

        const res = await client.post(
            "/auth/account/phone",
            {phoneNumber: "+79123456789"},
            withAuthHeader(client, token)
        );
        expect(res.status).toBe(200);
    });
});
