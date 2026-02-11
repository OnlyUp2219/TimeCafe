import {describe, it, expect} from "vitest";
import {canUseS3, createPassword, createTestClient, createTestEmail, loginAndGetAccessToken, parseCallbackParams, registerUser} from "@tests/integration/api/helpers";

describe("/userprofile/S3/image", () => {
    it("returns 401 when unauthorized", async () => {
        if (!canUseS3()) return;
        if (typeof FormData === "undefined" || typeof Blob === "undefined") return;
        const {client} = createTestClient();
        const form = new FormData();
        const blob = new Blob(["test"], {type: "text/plain"});
        form.append("file", blob, "test.txt");
        const res = await client.post("/userprofile/S3/image/00000000-0000-0000-0000-000000000001", form);
        expect(res.status).toBe(401);
    });

    it("uploads and deletes photo when authorized", async () => {
        if (!canUseS3()) return;
        if (typeof FormData === "undefined" || typeof Blob === "undefined") return;

        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        const callbackUrl = registerRes.data?.callbackUrl as string | undefined;
        let userId = "";
        if (callbackUrl) {
            const parsed = parseCallbackParams(callbackUrl);
            userId = parsed.userId;
            if (parsed.userId && parsed.token) {
                await client.post("/auth/email/confirm", {userId: parsed.userId, token: parsed.token});
            }
        }

        const {token} = await loginAndGetAccessToken(client, email, password);
        expect(token).toBeTruthy();

        if (userId) {
            const form = new FormData();
            const blob = new Blob(["test"], {type: "text/plain"});
            form.append("file", blob, "test.txt");

            const uploadRes = await client.post(`/userprofile/S3/image/${userId}`, form, {
                headers: {Authorization: `Bearer ${token}`},
            });
            expect(uploadRes.status).toBeGreaterThanOrEqual(200);
            expect(uploadRes.status).toBeLessThan(300);

            const deleteRes = await client.delete(`/userprofile/S3/image/${userId}`, {
                headers: {Authorization: `Bearer ${token}`},
            });
            expect(deleteRes.status).toBeGreaterThanOrEqual(200);
            expect(deleteRes.status).toBeLessThan(300);
        }
    });

    it("returns 200 for authorized get after upload", async () => {
        if (!canUseS3()) return;
        if (typeof FormData === "undefined" || typeof Blob === "undefined") return;

        const {client} = createTestClient();
        const email = createTestEmail();
        const password = createPassword();

        const registerRes = await registerUser(client, email, password);
        const callbackUrl = registerRes.data?.callbackUrl as string | undefined;
        let userId = "";
        if (callbackUrl) {
            const parsed = parseCallbackParams(callbackUrl);
            userId = parsed.userId;
            if (parsed.userId && parsed.token) {
                await client.post("/auth/email/confirm", {userId: parsed.userId, token: parsed.token});
            }
        }

        const {token} = await loginAndGetAccessToken(client, email, password);
        expect(token).toBeTruthy();

        if (!userId) return;

        const form = new FormData();
        const blob = new Blob([new Uint8Array([137, 80, 78, 71])], {type: "image/png"});
        form.append("file", blob, "test.png");

        const uploadRes = await client.post(`/userprofile/S3/image/${userId}`, form, {
            headers: {Authorization: `Bearer ${token}`},
        });
        expect(uploadRes.status).toBeGreaterThanOrEqual(200);
        expect(uploadRes.status).toBeLessThan(300);

        const getUnauthorized = await client.get(`/userprofile/S3/image/${userId}`);
        expect(getUnauthorized.status).toBe(401);

        const getAuthorized = await client.get(`/userprofile/S3/image/${userId}`, {
            headers: {Authorization: `Bearer ${token}`},
            responseType: "arraybuffer",
        });
        expect(getAuthorized.status).toBe(200);
        expect(getAuthorized.data).toBeTruthy();
    });
});
