import {describe, expect, it} from "vitest";
import {createTestClient} from "@tests/integration/api/helpers";
import {createAuthorizedContext} from "@tests/integration/api/venue/authContext";

const tariffId = "00000000-0000-0000-0000-000000000001";

const payload = {
    tariffId,
    name: "Test Tariff",
    description: "Test",
    pricePerMinute: 10,
    billingType: 1,
    themeId: null,
    isActive: true,
};

describe("PUT /venue/tariffs/{id}", () => {
    it("returns endpoint result when authorized", async () => {
        const {client, headers} = await createAuthorizedContext();
        const res = await client.put(`/venue/tariffs/${tariffId}`, payload, {headers});
        expect(res.status).not.toBe(401);
    });

    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.put(`/venue/tariffs/${tariffId}`, payload);
        expect(res.status).toBe(401);
    });
});