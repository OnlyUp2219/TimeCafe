import { CURRENCY_SYMBOL } from "@shared/const/currency";
import { NO_DATA } from "@shared/const/placeholders";

export const formatMoneyByN = (value: number, maximumFractionDigits = 2): string => {
    try {
        return new Intl.NumberFormat("be-BY", {
            style: "currency",
            currency: "BYN",
            maximumFractionDigits,
        }).format(value);
    } catch {
        return `${value.toFixed(maximumFractionDigits)} ${CURRENCY_SYMBOL}`;
    }
};

export const formatMoney = (v: number | null | undefined, maximumFractionDigits = 2): string => {
    if (v == null) return NO_DATA;
    return formatMoneyByN(v, maximumFractionDigits);
};

export const formatByn = (value: number, maximumFractionDigits = 2): string => {
    return formatMoneyByN(value, maximumFractionDigits);
};
