import {describe, expect, it} from "vitest";
import {createTestClient} from "@tests/integration/api/helpers";
import {createAuthorizedContext} from "@tests/integration/api/venue/authContext";

describe("GET /venue/tariffs/active", () => {
    it("returns tariffs when authorized", async () => {
        const {client, headers} = await createAuthorizedContext();
        const res = await client.get("/venue/tariffs/active", {headers});
        expect(res.status).not.toBe(401);
    });

    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.get("/venue/tariffs/active");
        expect(res.status).toBe(401);
    });
});