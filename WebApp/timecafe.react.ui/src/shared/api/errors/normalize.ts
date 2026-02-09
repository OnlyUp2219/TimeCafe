import type {AxiosError} from "axios";
import {isAxiosError} from "axios";
import type {ApiError, ApiErrorItem} from "@api/errors/types";
import {isApiError} from "@api/errors/types";

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

export const normalizeAxiosError = (error: AxiosError): ApiError => {
    const statusCode = error.response?.status ?? 0;
    const data = error.response?.data;

    if (data) {
        const cqrs = normalizeFromCqrsV2(data, statusCode);
        if (cqrs) return cqrs;

        const pd = normalizeFromProblemDetails(data, statusCode);
        if (pd) return pd;

        if (typeof data === "string") {
            return {statusCode, message: data, raw: data};
        }

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
    }

    if (statusCode === 401) {
        return {
            statusCode: 401,
            code: "Unauthorized",
            message: "Unauthorized",
            raw: error,
        };
    }

    if (statusCode === 403) {
        return {
            statusCode: 403,
            code: "Forbidden",
            message: "Forbidden",
            raw: error,
        };
    }

    return {
        statusCode,
        code: undefined,
        message: error.message || "Произошла ошибка сети",
        raw: error,
    };
};

export const normalizeUnknownError = (error: unknown): ApiError => {
    if (isApiError(error)) {
        return error;
    }

    if (isAxiosError(error)) {
        return normalizeAxiosError(error);
    }

    if (error instanceof Error) {
        return {statusCode: 0, message: error.message, raw: error};
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
