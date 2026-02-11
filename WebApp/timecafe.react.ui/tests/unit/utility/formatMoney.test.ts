import {afterEach, describe, expect, it, vi} from "vitest";
import {formatMoneyByN} from "@utility/formatMoney";

describe("formatMoneyByN", () => {
    const originalNumberFormat = Intl.NumberFormat;

    afterEach(() => {
        Intl.NumberFormat = originalNumberFormat;
    });

    it("uses Intl formatter when available", () => {
        Intl.NumberFormat = vi.fn(() => ({
            format: () => "12,30 BYN",
        })) as unknown as typeof Intl.NumberFormat;

        expect(formatMoneyByN(12.3)).toBe("12,30 BYN");
    });

    it("falls back to fixed format on Intl errors", () => {
        Intl.NumberFormat = vi.fn(() => {
            throw new Error("Intl not available");
        }) as unknown as typeof Intl.NumberFormat;

        expect(formatMoneyByN(12.345, 1)).toBe("12.3 BYN");
    });
});
