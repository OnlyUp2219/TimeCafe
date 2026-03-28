import {afterEach, describe, expect, it, vi} from "vitest";
import {formatMoneyByN} from "@utility/formatMoney";

describe("formatMoneyByN", () => {
    const originalNumberFormat = Intl.NumberFormat;

    afterEach(() => {
        Intl.NumberFormat = originalNumberFormat;
    });

    it("uses Intl formatter when available", () => {
        class MockNumberFormat {
            format() {
                return "12,30 ₽";
            }
        }
        Intl.NumberFormat = MockNumberFormat as unknown as typeof Intl.NumberFormat;

        expect(formatMoneyByN(12.3)).toBe("12,30 ₽");
    });

    it("falls back to fixed format on Intl errors", () => {
        class ThrowingNumberFormat {
            constructor() {
                throw new Error("Intl not available");
            }
        }
        Intl.NumberFormat = ThrowingNumberFormat as unknown as typeof Intl.NumberFormat;

        expect(formatMoneyByN(12.345, 1)).toBe("12.3 ₽");
    });
});
