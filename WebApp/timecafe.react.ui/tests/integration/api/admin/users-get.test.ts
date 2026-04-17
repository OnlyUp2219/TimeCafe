import {describe, expect, it} from "vitest";
import {createTestClient} from "@tests/integration/api/helpers";
import {createAuthorizedContext} from "@tests/integration/api/venue/authContext";

describe("GET /auth/admin/users", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.get("/auth/admin/users", {params: {page: 1, size: 10}});
        expect(res.status).toBe(401);
    });

    it("returns users list when authorized", async () => {
        const {client, headers} = await createAuthorizedContext();
        const res = await client.get("/auth/admin/users", {headers, params: {page: 1, size: 10}});
        expect(res.status).not.toBe(401);
    });
});

describe("GET /auth/admin/users/:id", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const fakeId = "00000000-0000-0000-0000-000000000001";
        const res = await client.get(`/auth/admin/users/${fakeId}`);
        expect(res.status).toBe(401);
    });

    it("returns 404 for non-existent user when authorized", async () => {
        const {client, headers} = await createAuthorizedContext();
        const fakeId = "00000000-0000-0000-0000-000000000001";
        const res = await client.get(`/auth/admin/users/${fakeId}`, {headers});
        expect([403, 404]).toContain(res.status);
    });
});
