import {describe, expect, it} from "vitest";
import {createTestClient} from "@tests/integration/api/helpers";
import {createAuthorizedContext} from "@tests/integration/api/venue/authContext";

const tariffId = "00000000-0000-0000-0000-000000000001";

describe("DELETE /venue/tariffs/{tariffId}", () => {
    it("returns endpoint result when authorized", async () => {
        const {client, headers} = await createAuthorizedContext();
        const res = await client.delete(`/venue/tariffs/${tariffId}`, {headers});
        expect(res.status).not.toBe(401);
    });

    it("returns 401 when unauthorized", async () => {
        const {client} = createTestClient();
        const res = await client.delete(`/venue/tariffs/${tariffId}`);
        expect(res.status).toBe(401);
    });
});