import type {AxiosError} from "axios";
import {isAxiosError} from "axios";
import type {ApiError, ApiErrorItem} from "@api/errors/types";
import {isApiError} from "@api/errors/types";
import {extractRtkError} from "@api/errors/extractRtkError";

const asRecord = (value: unknown): Record<string, unknown> | null => {
    if (!value || typeof value !== "object" || Array.isArray(value)) return null;
    return value as Record<string, unknown>;
};

const flattenUnknown = (value: unknown): string[] => {
    if (value == null) return [];
    if (typeof value === "string") return [value];
    if (Array.isArray(value)) return value.flatMap(flattenUnknown);
    if (typeof value === "object") {
        return Object.values(value as Record<string, unknown>).flatMap(flattenUnknown);
    }
    return [String(value)];
};

const toApiErrorItemsFromAspNetValidation = (errors: unknown): ApiErrorItem[] | undefined => {
    if (!errors || typeof errors !== "object" || Array.isArray(errors)) return undefined;

    const list: ApiErrorItem[] = [];
    for (const value of Object.values(errors as Record<string, unknown>)) {
        const messages = flattenUnknown(value)
            .map(s => s.trim())
            .filter(Boolean);
        for (const message of messages) {
            list.push({code: "Validation", message});
        }
    }

    return list.length ? list : undefined;
};

const normalizeFromCqrsV2 = (data: unknown, fallbackStatusCode: number): ApiError | null => {
    const obj = asRecord(data);
    if (!obj) return null;

    const statusCode = typeof obj.statusCode === "number" ? obj.statusCode : fallbackStatusCode;
    const message = typeof obj.message === "string" ? obj.message : "Произошла ошибка";
    const code = typeof obj.code === "string" ? obj.code : undefined;

    let errors: ApiErrorItem[] | undefined;
    if (Array.isArray(obj.errors)) {
        const list = obj.errors
            .map((item: unknown): ApiErrorItem | null => {
                const errObj = asRecord(item);
                if (!errObj) return null;

                const msg =
                    typeof errObj.message === "string"
                        ? errObj.message
                        : typeof errObj.description === "string"
                            ? errObj.description
                            : null;
                if (!msg) return null;

                const errCode = typeof errObj.code === "string" ? errObj.code : undefined;
                return errCode ? {code: errCode, message: msg} : {message: msg};
            })
            .filter((x): x is ApiErrorItem => x !== null);

        errors = list.length ? list : undefined;
    }

    return {
        statusCode,
        code,
        message,
        errors,
        raw: obj,
    };
};

const normalizeFromProblemDetails = (data: unknown, fallbackStatusCode: number): ApiError | null => {
    const obj = asRecord(data);
    if (!obj) return null;

    const statusCode = typeof obj.status === "number" ? obj.status : fallbackStatusCode;
    const title = typeof obj.title === "string" ? obj.title : undefined;
    const detail = typeof obj.detail === "string" ? obj.detail : undefined;
    const message = detail || title || "Произошла ошибка";

    const errors = toApiErrorItemsFromAspNetValidation(obj.errors);

    return {
        statusCode,
        code: undefined,
        message,
        errors,
        raw: obj,
    };
};

const normalizeFromLegacy = (data: unknown, fallbackStatusCode: number): ApiError | null => {
    const obj = asRecord(data);
    if (!obj) return null;

    // Проверяем наличие признаков "старого" формата
    const hasSuccess = typeof obj.success === "boolean";
    const hasErrorField = typeof obj.error === "string";
    const hasMessage = typeof obj.message === "string";

    if (!hasSuccess && !hasErrorField) return null;

    const statusCode = typeof obj.statusCode === "number" ? obj.statusCode : fallbackStatusCode;
    const message = (obj.error as string) || (obj.message as string) || "Ошибка (Legacy format)";
    const code = "LegacyError";

    return {
        statusCode,
        code,
        message,
        raw: obj,
    };
};

export const normalizeAxiosError = (error: AxiosError): ApiError => {
    const statusCode = error.response?.status ?? 0;
    const data = error.response?.data;

    if (data) {
        // 1. Попытка нормализации из CQRS V2 (наш текущий стандарт)
        const cqrs = normalizeFromCqrsV2(data, statusCode);
        if (cqrs) return cqrs;

        // 2. Попытка нормализации из ASP.NET ProblemDetails
        const pd = normalizeFromProblemDetails(data, statusCode);
        if (pd) return pd;

        // 3. Попытка нормализации из Legacy форматов
        const legacy = normalizeFromLegacy(data, statusCode);
        if (legacy) return legacy;

        // 4. Попытка извлечь message напрямую из объекта
        if (typeof data === "object") {
            const obj = asRecord(data);
            const maybeMessage = obj?.message;
            if (typeof maybeMessage === "string") return {statusCode, message: maybeMessage, raw: data};

            const flat = flattenUnknown(data)
                .map(s => s.trim())
                .filter(Boolean);
            if (flat.length) {
                return {statusCode, message: flat.join(" "), raw: data};
            }
        }

        if (typeof data === "string") {
            return {statusCode, message: data, raw: data};
        }
    }

    if (statusCode === 401) {
        return {
            statusCode: 401,
            code: "Unauthorized",
            message: "Сессия истекла. Пожалуйста, войдите снова.",
            raw: error,
        };
    }

    if (statusCode === 403) {
        return {
            statusCode: 403,
            code: "Forbidden",
            message: "У вас недостаточно прав для этого действия.",
            raw: error,
        };
    }

    return {
        statusCode,
        code: "NetworkError",
        message: error.message || "Произошла ошибка сети",
        raw: error,
    };
};

export const normalizeUnknownError = (error: unknown): ApiError => {
    if (isApiError(error)) {
        return error;
    }

    const rtkError = extractRtkError(error as never);
    if (rtkError) {
        return rtkError;
    }

    if (isAxiosError(error)) {
        return normalizeAxiosError(error);
    }

    if (error instanceof Error) {
        const message = error.message || "Неизвестная ошибка";
        const normalizedMessage = /Failed to fetch|NetworkError|fetch/i.test(message)
            ? "Не удалось подключиться к серверу. Проверьте интернет или повторите позже."
            : message;
        return {statusCode: 0, message: normalizedMessage, raw: error};
    }

    if (typeof error === "string") {
        return {statusCode: 0, message: error, raw: error};
    }

    if (error && typeof error === "object") {
        const obj = asRecord(error);
        const maybeMessage = obj?.message;
        if (typeof maybeMessage === "string") return {statusCode: 0, message: maybeMessage, raw: error};

        const flat = flattenUnknown(error)
            .map(s => s.trim())
            .filter(Boolean);
        if (flat.length) {
            return {statusCode: 0, message: flat.join(" "), raw: error};
        }
    }

    return {statusCode: 0, message: "Неизвестная ошибка", raw: error};
};
