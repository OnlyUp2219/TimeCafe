import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, loginAndGetRefreshToken, registerAndConfirm, buildRefreshCookieHeader} from "@tests/integration/api/helpers";

describe("/auth/logout", () => {
    it("returns revoked false when cookie missing", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/logout", null);
        expect(res.status).toBe(200);
        expect(res.data?.revoked).toBe(false);
    });

    it("returns revoked true when cookie valid", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {refreshToken} = await loginAndGetRefreshToken(client, email, password);
        expect(refreshToken).toBeTruthy();

        const res = await client.post(
            "/auth/logout",
            null,
            {headers: {Cookie: buildRefreshCookieHeader(refreshToken)}}
        );
        expect(res.status).toBe(200);
        expect(res.data?.revoked).toBe(true);
    });

    it("returns revoked false when cookie reused", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {refreshToken} = await loginAndGetRefreshToken(client, email, password);

        await client.post(
            "/auth/logout",
            null,
            {headers: {Cookie: buildRefreshCookieHeader(refreshToken)}}
        );

        const res = await client.post(
            "/auth/logout",
            null,
            {headers: {Cookie: buildRefreshCookieHeader(refreshToken)}}
        );

        expect(res.status).toBe(200);
        expect(res.data?.revoked).toBe(false);
    });
});
