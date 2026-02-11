import {beforeEach, describe, expect, it, vi} from "vitest";

vi.mock("@utility/formatMoney", () => ({
    formatMoneyByN: vi.fn(),
}));

import {calcVisitEstimate} from "@utility/visitEstimate";
import {formatMoneyByN} from "@utility/formatMoney";
import type {BillingType} from "@app-types/tariff";

describe("calcVisitEstimate", () => {
    beforeEach(() => {
        vi.mocked(formatMoneyByN).mockReturnValue("10 BYN");
    });

    it("calculates per-minute estimates", () => {
        const result = calcVisitEstimate(15.9, "PerMinute" as BillingType, 2);

        expect(result.total).toBe(30);
        expect(result.chargedMinutes).toBe(15);
        expect(result.breakdown).toBe("10 BYN / мин × 15 мин");
    });

    it("calculates hourly estimates", () => {
        vi.mocked(formatMoneyByN).mockReturnValue("90 BYN");
        const result = calcVisitEstimate(61, "Hourly" as BillingType, 1.5);

        expect(result.total).toBe(180);
        expect(result.chargedHours).toBe(2);
        expect(result.breakdown).toBe("90 BYN / час × 2 ч");
    });

    it("guards against negative price", () => {
        vi.mocked(formatMoneyByN).mockReturnValue("0 BYN");
        const result = calcVisitEstimate(5, "PerMinute" as BillingType, -1);

        expect(result.total).toBe(0);
        expect(result.chargedMinutes).toBe(5);
        expect(result.breakdown).toBe("0 BYN / мин × 5 мин");
    });
});
