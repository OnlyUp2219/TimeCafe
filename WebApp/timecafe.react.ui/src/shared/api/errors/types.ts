export interface ApiErrorItem {
    code?: string;
    message: string;
}

export interface ApiError {
    statusCode: number;
    code?: string;
    message: string;
    errors?: ApiErrorItem[];
    raw?: unknown;
}

export const isApiError = (value: unknown): value is ApiError => {
    if (!value || typeof value !== "object") return false;
    const v = value as Record<string, unknown>;
    return typeof v.statusCode === "number" && typeof v.message === "string";
};
