import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, registerUser} from "../helpers";

describe("/auth/forgot-password-link", () => {
    it("returns callbackUrl when user exists", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        expect(registerRes.status).toBeGreaterThanOrEqual(200);
        expect(registerRes.status).toBeLessThan(300);

        const res = await client.post("/auth/forgot-password-link-mock", {email});
        expect(res.status).toBe(200);
        expect(res.data?.callbackUrl).toBeTruthy();
    });

    it("returns success when user does not exist", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();

        const res = await client.post("/auth/forgot-password-link-mock", {email});
        expect(res.status).toBe(200);
        expect(res.data?.callbackUrl).toBeFalsy();
    });

    it("returns 422 for invalid email", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/forgot-password-link-mock", {email: "invalid"});
        expect(res.status).toBe(422);
    });
});
