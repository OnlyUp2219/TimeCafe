export const normalizeDate = (value: unknown): Date | undefined => {
    if (!value) return undefined;
    if (value instanceof Date) return Number.isNaN(value.getTime()) ? undefined : value;
    if (typeof value === "string" || typeof value === "number") {
        const d = new Date(value);
        return Number.isNaN(d.getTime()) ? undefined : d;
    }
    return undefined;
};

export const normalizeDateForApi = (value: unknown): string | null => {
    const date = normalizeDate(value);
    if (!date) return null;
    return date.toISOString().slice(0, 10);
};

export const normalizeBirthDateForApi = (value: string | undefined): string | null => {
    const trimmed = (value ?? "").trim();
    if (!trimmed) return null;

    const m = /^\d{4}-\d{2}-\d{2}/.exec(trimmed);
    if (!m) return null;

    return trimmed.slice(0, 10);
};
