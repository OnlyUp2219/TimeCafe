import { NO_DATA } from "@shared/const/placeholders";

export const getGuestsWord = (count: number): string => {
    const mod10 = count % 10;
    const mod100 = count % 100;
    if (mod10 === 1 && mod100 !== 11) return "гость";
    if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return "гостя";
    return "гостей";
};

export const formatRoundingRule = (rule: string | null | undefined): string => {
    if (!rule) return NO_DATA;
    if (rule === "FiveMinutes") return "до 5 мин.";
    if (rule === "FifteenMinutes") return "до 15 мин.";
    if (rule === "SixtyMinutes") return "до 1 ч.";
    return rule;
};

export const formatMoney = (v: number | null | undefined, currencySymbol: string = "₽"): string => {
    if (v == null) return NO_DATA;
    return `${v.toFixed(2)} ${currencySymbol}`;
};
