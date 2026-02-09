import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, getPhoneGenerateEndpoint, getPhoneVerifyEndpoint, loginAndGetAccessToken, registerAndConfirm, withAuthHeader} from "../helpers";

describe("/auth/twilio/verifySMS", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.post(getPhoneVerifyEndpoint(), {phoneNumber: "+79123456789", code: "123456"});
        expect(res.status).toBe(401);
    });

    it("returns 400 for invalid code", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {token} = await loginAndGetAccessToken(client, email, password);

        const res = await client.post(
            getPhoneVerifyEndpoint(),
            {phoneNumber: "+79123456789", code: "wrong"},
            withAuthHeader(client, token)
        );
        expect(res.status).toBe(400);
    });

    it("verifies phone with valid code", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {token} = await loginAndGetAccessToken(client, email, password);

        const generateRes = await client.post(
            getPhoneGenerateEndpoint(),
            {phoneNumber: "+79123456789"},
            withAuthHeader(client, token)
        );
        const code = generateRes.data?.token as string | undefined;
        expect(code).toBeTruthy();

        const res = await client.post(
            getPhoneVerifyEndpoint(),
            {phoneNumber: "+79123456789", code},
            withAuthHeader(client, token)
        );
        expect(res.status).toBe(200);
    });
});
