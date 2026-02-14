import {describe, expect, it} from "vitest";
import {createTestClient} from "@tests/integration/api/helpers";
import {createAuthorizedContext} from "@tests/integration/api/venue/authContext";

const payload = {
    name: "Test Tariff",
    description: "Test",
    pricePerMinute: 10,
    billingType: 1,
    themeId: null,
    isActive: true,
};

describe("POST /venue/tariffs", () => {
    it("returns endpoint result when authorized", async () => {
        const {client, headers} = await createAuthorizedContext();
        const res = await client.post("/venue/tariffs", payload, {headers});
        expect(res.status).not.toBe(401);
    });

    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.post("/venue/tariffs", payload);
        expect(res.status).toBe(401);
    });
});