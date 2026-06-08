import {BillingType as BillingTypeEnum, type BillingType} from "@app-types/tariff";
import {formatMoneyByN} from "@utility/formatMoney";

export type VisitEstimate = {
    total: number;
    breakdown: string;
    chargedHours?: number;
    chargedMinutes?: number;
    baseTotal: number;
    discountTotal: number;
    appliedDiscountPercent: number;
    isDiscounted: boolean;
};

export const calcVisitEstimate = (
    elapsedMinutes: number,
    billingType: BillingType,
    pricePerMinute: number,
    minSessionMinutes: number | null = null,
    roundingRule: string | null = null,
    globalDiscount = 0,
    tariffDiscount = 0,
    personalDiscount = 0,
    maxDiscountPercent = 50
): VisitEstimate => {
    const actualDuration = elapsedMinutes;
    const safePricePerMinute = Math.max(0, pricePerMinute);

    let duration = actualDuration;
    if (minSessionMinutes && duration < minSessionMinutes) {
        duration = minSessionMinutes;
    }

    let roundInterval = 1;
    if (roundingRule === "FiveMinutes") roundInterval = 5;
    else if (roundingRule === "FifteenMinutes") roundInterval = 15;
    else if (roundingRule === "SixtyMinutes") roundInterval = 60;

    if (roundInterval > 1) {
        duration = Math.ceil(duration / roundInterval) * roundInterval;
    }

    const bestPromotion = Math.max(globalDiscount, tariffDiscount);
    const appliedDiscountPercent = Math.min(bestPromotion + personalDiscount, maxDiscountPercent);
    const isDiscounted = appliedDiscountPercent > 0;

    let baseTotal = 0;
    let breakdown = "";
    let chargedMinutes: number | undefined;
    let chargedHours: number | undefined;

    if (billingType === BillingTypeEnum.Hourly) {
        const hours = Math.max(1, Math.ceil(duration / 60));
        const pricePerHour = safePricePerMinute * 60;
        baseTotal = hours * pricePerHour;
        chargedHours = hours;
        breakdown = `${formatMoneyByN(pricePerHour)} / час × ${hours} ч`;
    } else {
        baseTotal = duration * safePricePerMinute;
        chargedMinutes = duration;
        breakdown = `${formatMoneyByN(safePricePerMinute)} / мин × ${duration} мин`;
    }

    const total = baseTotal * (1 - appliedDiscountPercent / 100);

    return {
        total,
        baseTotal,
        discountTotal: baseTotal - total,
        appliedDiscountPercent,
        isDiscounted,
        chargedHours,
        chargedMinutes,
        breakdown,
    };
};
