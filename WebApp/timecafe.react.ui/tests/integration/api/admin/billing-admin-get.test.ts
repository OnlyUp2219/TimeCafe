import {describe, expect, it} from "vitest";
import {createTestClient} from "@tests/integration/api/helpers";
import {createAuthorizedContext} from "@tests/integration/api/venue/authContext";

describe("GET /billing/admin/balances", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.get("/billing/admin/balances", {params: {page: 1, pageSize: 20}});
        expect(res.status).toBe(401);
    });

    it("returns response when authorized", async () => {
        const {client, headers} = await createAuthorizedContext();
        const res = await client.get("/billing/admin/balances", {headers, params: {page: 1, pageSize: 20}});
        expect(res.status).not.toBe(401);
    });
});

describe("GET /billing/admin/transactions", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.get("/billing/admin/transactions", {params: {page: 1, pageSize: 20}});
        expect(res.status).toBe(401);
    });

    it("returns response when authorized", async () => {
        const {client, headers} = await createAuthorizedContext();
        const res = await client.get("/billing/admin/transactions", {headers, params: {page: 1, pageSize: 20}});
        expect(res.status).not.toBe(401);
    });
});

describe("GET /billing/admin/payments", () => {
    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.get("/billing/admin/payments", {params: {page: 1, pageSize: 20}});
        expect(res.status).toBe(401);
    });

    it("returns response when authorized", async () => {
        const {client, headers} = await createAuthorizedContext();
        const res = await client.get("/billing/admin/payments", {headers, params: {page: 1, pageSize: 20}});
        expect(res.status).not.toBe(401);
    });
});
