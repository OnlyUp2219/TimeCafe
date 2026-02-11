import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, registerAndConfirm, registerUser} from "@tests/integration/api/helpers";

describe("/auth/login-jwt-v2", () => {
    it("returns emailConfirmed false when email not confirmed", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        expect(registerRes.status).toBeGreaterThanOrEqual(200);
        expect(registerRes.status).toBeLessThan(300);

        const loginRes = await client.post("/auth/login-jwt-v2", {email, password});
        expect(loginRes.status).toBe(200);
        expect(loginRes.data?.emailConfirmed).toBe(false);
        expect(loginRes.data?.accessToken).toBeFalsy();
    });

    it("returns accessToken and refresh cookie when confirmed", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);

        const loginRes = await client.post("/auth/login-jwt-v2", {email, password});
        expect(loginRes.status).toBe(200);
        expect(loginRes.data?.accessToken).toBeTruthy();
        const setCookie = loginRes.headers?.["set-cookie"] as string[] | undefined;
        expect(setCookie?.some(c => c.startsWith("refresh_token="))).toBe(true);
    });

    it("returns 400 for invalid credentials", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);

        const loginRes = await client.post("/auth/login-jwt-v2", {email, password: "wrong"});
        expect(loginRes.status).toBe(400);
    });

    it("returns 422 for invalid email format", async () => {
        const {client} = createTestClient();
        const loginRes = await client.post("/auth/login-jwt-v2", {email: "invalid", password: "x"});
        expect(loginRes.status).toBe(422);
    });
});
