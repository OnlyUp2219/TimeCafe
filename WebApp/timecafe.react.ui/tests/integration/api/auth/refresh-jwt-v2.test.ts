import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, loginAndGetRefreshToken, registerAndConfirm, buildRefreshCookieHeader} from "../helpers";

describe("/auth/refresh-jwt-v2", () => {
    it("returns 401 when cookie missing", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/refresh-jwt-v2", null);
        expect(res.status).toBe(401);
    });

    it("returns access token and rotates cookie", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);
        const {refreshToken} = await loginAndGetRefreshToken(client, email, password);
        expect(refreshToken).toBeTruthy();

        const refreshRes = await client.post(
            "/auth/refresh-jwt-v2",
            null,
            {headers: {Cookie: buildRefreshCookieHeader(refreshToken)}}
        );
        expect(refreshRes.status).toBe(200);
        expect(refreshRes.data?.accessToken).toBeTruthy();
        const setCookie = refreshRes.headers?.["set-cookie"] as string[] | undefined;
        expect(setCookie?.some(c => c.startsWith("refresh_token="))).toBe(true);
    });
});
