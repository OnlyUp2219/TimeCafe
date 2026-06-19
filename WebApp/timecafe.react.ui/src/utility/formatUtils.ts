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

export { formatMoney, formatMoneyByN, formatRub } from "./formatMoney";


export const safeParseJson = (str?: string | null): any => {
    if (!str) return null;
    try {
        return JSON.parse(str);
    } catch {
        return str;
    }
};

export const formatDurationMs = (ms: number): string => {
    if (ms < 1000) return `${ms} мс`;
    if (ms < 60000) return `${(ms / 1000).toFixed(2)} с`;
    const minutes = Math.floor(ms / 60000);
    const seconds = ((ms % 60000) / 1000).toFixed(0);
    return `${minutes} мин ${seconds} с`;
};

