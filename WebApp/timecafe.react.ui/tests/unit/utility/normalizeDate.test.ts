import {describe, expect, it} from "vitest";
import {normalizeDate} from "@utility/normalizeDate";

describe("normalizeDate", () => {
    it("returns undefined for empty values", () => {
        expect(normalizeDate(null)).toBeUndefined();
        expect(normalizeDate(undefined)).toBeUndefined();
    });

    it("returns undefined for invalid dates", () => {
        expect(normalizeDate(new Date("invalid"))).toBeUndefined();
        expect(normalizeDate("invalid")).toBeUndefined();
    });

    it("returns Date for valid inputs", () => {
        const fromString = normalizeDate("2024-01-01");
        const fromNumber = normalizeDate(1700000000000);

        expect(fromString).toBeInstanceOf(Date);
        expect(fromString?.getTime()).not.toBeNaN();
        expect(fromNumber).toBeInstanceOf(Date);
        expect(fromNumber?.getTime()).not.toBeNaN();
    });
});
