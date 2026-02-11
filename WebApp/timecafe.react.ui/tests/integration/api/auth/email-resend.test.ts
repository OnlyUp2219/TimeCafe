import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, registerAndConfirm, registerUser} from "@tests/integration/api/helpers";

describe("/auth/email/resend", () => {
    it("returns callbackUrl for unconfirmed email", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        expect(registerRes.status).toBeGreaterThanOrEqual(200);
        expect(registerRes.status).toBeLessThan(300);

        const res = await client.post("/auth/email/resend-mock", {email});
        expect(res.status).toBe(200);
        expect(res.data?.callbackUrl).toBeTruthy();
    });

    it("returns 400 for confirmed email", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        await registerAndConfirm(client, email, password);

        const res = await client.post("/auth/email/resend-mock", {email});
        expect(res.status).toBe(400);
    });

    it("returns 401 for nonexistent email", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/email/resend-mock", {email: createTestEmail()});
        expect(res.status).toBe(401);
    });

    it("returns 422 for invalid email", async () => {
        const {client} = createTestClient();
        const res = await client.post("/auth/email/resend-mock", {email: "invalid"});
        expect(res.status).toBe(422);
    });
});
