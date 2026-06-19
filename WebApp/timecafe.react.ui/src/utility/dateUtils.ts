import { NO_DATA } from "@shared/const/placeholders";

export const formatDateTime = (iso: string | null | undefined): string => {
    if (!iso) return NO_DATA;
    const d = new Date(iso);
    return d.toLocaleString("ru-RU", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit"
    });
};

export const getRelativeTime = (iso: string | null | undefined): string => {
    if (!iso) return "";
    const diffMs = Date.now() - new Date(iso).getTime();
    const diffMins = Math.floor(diffMs / 60000);
    if (diffMins < 1) return "только что";
    if (diffMins < 60) return `${diffMins} мин. назад`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} ч. назад`;
    return "";
};

const pad2 = (value: number) => value.toString().padStart(2, "0");

export const formatTimeHHmm = (hhmm: string | null | undefined, fallback: Date): string => {
    if (hhmm && /^\d{2}:\d{2}$/.test(hhmm)) return hhmm;
    return `${pad2(fallback.getHours())}:${pad2(fallback.getMinutes())}`;
};

export const formatDate = (date: string | Date | null | undefined): string => {
    if (!date) return NO_DATA;
    const d = typeof date === "string" ? new Date(date) : date;
    if (Number.isNaN(d.getTime())) return NO_DATA;
    return d.toLocaleDateString("ru-RU", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric"
    });
};

export const formatDateShort = (date: Date | string | null | undefined): string => {
    if (!date) return NO_DATA;
    const d = date instanceof Date ? date : new Date(date);
    if (Number.isNaN(d.getTime())) return NO_DATA;
    return d.toLocaleDateString("ru-RU", {
        day: "2-digit",
        month: "2-digit"
    });
};
