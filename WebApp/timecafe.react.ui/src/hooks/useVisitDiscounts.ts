import { useMemo } from "react";

export interface VisitDiscounts {
    globalDiscount: number;
    tariffDiscount: number;
    personalDiscount: number;
}

export const getVisitDiscounts = (
    promotions: any[] | undefined,
    loyalty: any | undefined,
    tariffId?: string | null
): VisitDiscounts => {
    const nowTime = Date.now();
    
    const activePromotions = promotions?.filter(p =>
        p.isActive &&
        (!p.validFrom || new Date(p.validFrom).getTime() <= nowTime) &&
        (!p.validTo || new Date(p.validTo).getTime() >= nowTime)
    ) ?? [];

    const globalDiscount = activePromotions.length > 0
        ? Math.max(0, ...activePromotions.filter(p => p.type === 1).map(p => p.discountPercent ?? 0))
        : 0;

    const tariffDiscount = (tariffId && activePromotions.length > 0)
        ? Math.max(0, ...activePromotions.filter(p => p.type === 2 && p.tariffId === tariffId).map(p => p.discountPercent ?? 0))
        : 0;

    const personalDiscount = loyalty?.personalDiscountPercent ?? 0;

    return {
        globalDiscount,
        tariffDiscount,
        personalDiscount
    };
};

export const useVisitDiscounts = (
    promotions: any[] | undefined,
    loyalty: any | undefined,
    tariffId?: string | null
): VisitDiscounts => {
    return useMemo(() => {
        return getVisitDiscounts(promotions, loyalty, tariffId);
    }, [promotions, loyalty, tariffId]);
};
