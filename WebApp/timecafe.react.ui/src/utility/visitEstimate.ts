import type {BillingType} from "@app-types/tariff";
import {formatMoneyByN} from "@utility/formatMoney";

export type VisitEstimate = {
    total: number;
    breakdown: string;
    chargedHours?: number;
    chargedMinutes?: number;
};

export const calcVisitEstimate = (elapsedMinutes: number, billingType: BillingType, pricePerMinute: number): VisitEstimate => {
    const minutes = Math.max(1, Math.floor(elapsedMinutes));
    const safePricePerMinute = Math.max(0, pricePerMinute);

    if (billingType === "PerMinute") {
        return {
            total: minutes * safePricePerMinute,
            chargedMinutes: minutes,
            breakdown: `${formatMoneyByN(safePricePerMinute)} / мин × ${minutes} мин`,
        };
    }

    const pricePerHour = safePricePerMinute * 60;
    const hours = Math.max(1, Math.ceil(minutes / 60));

    return {
        total: hours * pricePerHour,
        chargedHours: hours,
        breakdown: `${formatMoneyByN(pricePerHour)} / час × ${hours} ч`,
    };
};
