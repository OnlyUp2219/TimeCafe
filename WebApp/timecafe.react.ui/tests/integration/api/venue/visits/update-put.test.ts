import {describe, expect, it} from "vitest";
import {createTestClient} from "@tests/integration/api/helpers";
import {createAuthorizedContext} from "@tests/integration/api/venue/authContext";

const payload = {
    visitId: "00000000-0000-0000-0000-000000000001",
    userId: "00000000-0000-0000-0000-000000000002",
    tariffId: "00000000-0000-0000-0000-000000000003",
    entryTime: new Date().toISOString(),
    exitTime: null,
    calculatedCost: null,
    status: 1,
};

describe("PUT /venue/visits", () => {
    it("returns endpoint result when authorized", async () => {
        const {client, headers} = await createAuthorizedContext();
        const res = await client.put("/venue/visits", payload, {headers});
        expect(res.status).not.toBe(401);
    });

    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.put("/venue/visits", payload);
        expect(res.status).toBe(401);
    });
});