import {describe, expect, it} from "vitest";
import {formatDurationSeconds} from "@utility/formatDurationSeconds";

describe("formatDurationSeconds", () => {
    it("formats under an hour", () => {
        expect(formatDurationSeconds(65)).toBe("1м 05с");
    });

    it("formats hours", () => {
        expect(formatDurationSeconds(3661)).toBe("1ч 01м 01с");
    });

    it("guards against negatives", () => {
        expect(formatDurationSeconds(-1)).toBe("0м 00с");
    });
});
