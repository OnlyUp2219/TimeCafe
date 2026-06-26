import {describe, expect, it, vi, afterEach} from "vitest";

vi.mock("@utility/formatMoney", () => {
    const mockFormatMoneyByN = vi.fn((value: number, digits = 2) => `${value.toFixed(digits)} Br`);
    return {
        formatMoneyByN: mockFormatMoneyByN,
        formatByn: vi.fn((value: number, digits = 2) => mockFormatMoneyByN(value, digits)),
    };
});

import {formatByn} from "@utility/formatMoney";

describe("formatByn", () => {
    afterEach(() => {
        vi.clearAllMocks();
    });

    it("formats integer amount with 2 fraction digits by default", () => {
        const result = formatByn(100);
        expect(result).toBe("100.00 Br");
    });

    it("formats decimal amount with specified fraction digits", () => {
        const result = formatByn(99.99, 2);
        expect(result).toBe("99.99 Br");
    });

    it("formats zero", () => {
        const result = formatByn(0);
        expect(result).toBe("0.00 Br");
    });

    it("formats large amount", () => {
        const result = formatByn(10000);
        expect(result).toBe("10000.00 Br");
    });
});
