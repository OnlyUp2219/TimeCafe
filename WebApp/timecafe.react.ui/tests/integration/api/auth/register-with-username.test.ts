import {describe, it, expect} from "vitest";
import {createPassword, createTestClient, createTestEmail, registerUser} from "../helpers";

describe("/auth/registerWithUsername", () => {
    it("returns callbackUrl for valid data", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const res = await registerUser(client, email, password);

        expect(res.status).toBeGreaterThanOrEqual(200);
        expect(res.status).toBeLessThan(300);
        expect(res.data?.callbackUrl).toBeTruthy();
    });

    it("returns 400 on duplicate email", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const first = await registerUser(client, email, password);
        expect(first.status).toBeGreaterThanOrEqual(200);
        expect(first.status).toBeLessThan(300);

        const second = await registerUser(client, email, password);
        expect(second.status).toBe(400);
    });

    it("returns 422 on invalid email", async () => {
        const {client} = createTestClient();
        const res = await registerUser(client, "invalid-email", createPassword());
        expect(res.status).toBe(422);
    });

    it("returns 400 on weak password", async () => {
        const {client} = createTestClient();
        const email = createTestEmail();
        const res = await registerUser(client, email, "short");
        expect(res.status).toBe(400);
    });
});
