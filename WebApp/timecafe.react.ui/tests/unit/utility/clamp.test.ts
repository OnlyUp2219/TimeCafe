import {describe, expect, it} from "vitest";
import {clamp} from "@utility/clamp";

describe("clamp", () => {
    it("returns value within range", () => {
        expect(clamp(5, 1, 10)).toBe(5);
    });

    it("clamps below minimum", () => {
        expect(clamp(-1, 0, 3)).toBe(0);
    });

    it("clamps above maximum", () => {
        expect(clamp(10, 0, 3)).toBe(3);
    });
});
