import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, getPhoneGenerateEndpoint, loginAndGetAccessToken, registerAndConfirm, withAuthHeader} from "@tests/integration/api/helpers";

describe("/auth/twilio/generateSMS", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.post(getPhoneGenerateEndpoint(), {phoneNumber: "+79123456789"});
        expect(res.status).toBe(401);
    });

    it("returns 422 for invalid phone", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {token} = await loginAndGetAccessToken(client, email, password);

        const res = await client.post(
            getPhoneGenerateEndpoint(),
            {phoneNumber: "123"},
            withAuthHeader(client, token)
        );
        expect(res.status).toBe(422);
    });

    it("returns token for valid phone", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {token} = await loginAndGetAccessToken(client, email, password);

        const res = await client.post(
            getPhoneGenerateEndpoint(),
            {phoneNumber: "+79123456789"},
            withAuthHeader(client, token)
        );
        expect(res.status).toBe(200);
        expect(res.data?.token).toBeTruthy();
    });
});
