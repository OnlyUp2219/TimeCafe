import {describe, expect, it} from "vitest";
import {formatDurationMinutes} from "@utility/formatDurationMinutes";

describe("formatDurationMinutes", () => {
    it("formats minutes under an hour", () => {
        expect(formatDurationMinutes(59.9)).toBe("59 мин");
    });

    it("formats exact hours", () => {
        expect(formatDurationMinutes(60)).toBe("1 ч");
    });

    it("formats hours and minutes", () => {
        expect(formatDurationMinutes(61)).toBe("1 ч 1 мин");
    });

    it("guards against negatives", () => {
        expect(formatDurationMinutes(-5)).toBe("0 мин");
    });
});
