import {describe, expect, it, vi, afterEach} from "vitest";

vi.mock("@utility/formatMoney", () => ({
    formatMoneyByN: vi.fn((value: number, digits = 0) => `${value.toFixed(digits)} ₽`),
}));

import {formatRub} from "@utility/formatRub";

describe("formatRub", () => {
    afterEach(() => {
        vi.clearAllMocks();
    });

    it("formats integer amount with 0 fraction digits by default", () => {
        const result = formatRub(100);
        expect(result).toBe("100 ₽");
    });

    it("formats decimal amount with specified fraction digits", () => {
        const result = formatRub(99.99, 2);
        expect(result).toBe("99.99 ₽");
    });

    it("formats zero", () => {
        const result = formatRub(0);
        expect(result).toBe("0 ₽");
    });

    it("formats large amount", () => {
        const result = formatRub(10000);
        expect(result).toBe("10000 ₽");
    });
});
