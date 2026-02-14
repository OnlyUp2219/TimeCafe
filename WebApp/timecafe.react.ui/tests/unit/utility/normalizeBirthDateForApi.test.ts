import {describe, expect, it} from "vitest";
import {normalizeBirthDateForApi} from "@utility/normalizeDate";

describe("normalizeBirthDateForApi", () => {
    it("returns null for empty values", () => {
        expect(normalizeBirthDateForApi(undefined)).toBeNull();
        expect(normalizeBirthDateForApi("")).toBeNull();
        expect(normalizeBirthDateForApi("   ")).toBeNull();
    });

    it("returns null for invalid formats", () => {
        expect(normalizeBirthDateForApi("01.01.2000")).toBeNull();
        expect(normalizeBirthDateForApi("2000/01/01")).toBeNull();
        expect(normalizeBirthDateForApi("abc")).toBeNull();
    });

    it("returns yyyy-mm-dd for valid values", () => {
        expect(normalizeBirthDateForApi("2000-01-02")).toBe("2000-01-02");
        expect(normalizeBirthDateForApi("2000-01-02T15:30:00Z")).toBe("2000-01-02");
        expect(normalizeBirthDateForApi(" 2000-01-02 ")).toBe("2000-01-02");
    });
});