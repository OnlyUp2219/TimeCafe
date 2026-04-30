import type {SerializedError} from "@reduxjs/toolkit";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import type {ApiError, ApiErrorItem} from "@api/errors/types";

const asRecord = (value: unknown): Record<string, unknown> | null => {
    if (!value || typeof value !== "object" || Array.isArray(value)) return null;
    return value as Record<string, unknown>;
};

export const extractRtkError = (error: FetchBaseQueryError | SerializedError | undefined): ApiError | null => {
    if (!error) return null;

    if ("status" in error) {
        const fe = error as FetchBaseQueryError;

        if (typeof fe.status === "number") {
            const data = asRecord(fe.data);
            if (data) {
                const message = 
                    typeof data.message === "string" ? data.message :
                    typeof data.title === "string" ? data.title :
                    typeof data.detail === "string" ? data.detail :
                    `Ошибка сервера (${fe.status})`;
                
                const code = typeof data.code === "string" ? data.code : undefined;
                const statusCode = typeof data.statusCode === "number" ? data.statusCode : fe.status;

                let errors: ApiError["errors"] = [];

                if (Array.isArray(data.errors)) {
                    errors = data.errors
                        .map((item: unknown) => {
                            const obj = asRecord(item);
                            if (!obj) return null;
                            const msg = typeof obj.message === "string" ? obj.message
                                : typeof obj.description === "string" ? obj.description : null;
                            if (!msg) return null;
                            return {code: typeof obj.code === "string" ? obj.code : undefined, message: msg};
                        })
                        .filter(Boolean) as ApiError["errors"];
                } 
                else if (asRecord(data.errors)) {
                    const validationErrors = data.errors as Record<string, string[] | string>;
                    const errorArray: ApiErrorItem[] = errors || [];
                    Object.entries(validationErrors).forEach(([key, val]) => {
                        const messages = Array.isArray(val) ? val : [val];
                        messages.forEach(m => {
                            if (typeof m === "string") {
                                errorArray.push({ code: key, message: m });
                            }
                        });
                    });
                    errors = errorArray;
                }

                return {statusCode, code, message, errors, raw: fe.data};
            }

            return {statusCode: fe.status, message: `Ошибка сервера (${fe.status})`, raw: fe.data};
        }

        if (fe.status === "FETCH_ERROR") {
            return {statusCode: 0, code: "NetworkError", message: "Ошибка сети. Проверьте соединение или CORS.", raw: fe};
        }

        if (fe.status === "PARSING_ERROR") {
            return {statusCode: 0, code: "ParseError", message: "Ошибка обработки ответа сервера (Invalid JSON)", raw: fe};
        }

        if (fe.status === "CUSTOM_ERROR") {
            return {statusCode: 0, code: "CustomError", message: (fe as unknown as {error: string}).error || "Произошла ошибка", raw: fe};
        }
    }

    const se = error as SerializedError;
    return {statusCode: 0, message: se.message || "Неизвестная ошибка", raw: se};
};

export const getRtkErrorMessage = (error: FetchBaseQueryError | SerializedError | undefined): string => {
    const apiError = extractRtkError(error);
    if (!apiError) return "";

    if (apiError.statusCode === 401) return "Сессия истекла. Войдите снова.";
    
    if (apiError.errors?.length) {
        const messages = apiError.errors.map(e => e.message).filter(Boolean);
        if (messages.length) return messages.join(". ");
    }

    if (apiError.message && 
        apiError.message !== "Произошла ошибка" && 
        !apiError.message.startsWith("Ошибка сервера")) {
        return apiError.message;
    }

    return apiError.message || "Произошла ошибка";
};

export const getRtkValidationErrors = (error: FetchBaseQueryError | SerializedError | undefined): Record<string, string> => {
    const apiError = extractRtkError(error);
    if (!apiError?.errors?.length) return {};

    const result: Record<string, string> = {};
    for (const e of apiError.errors) {
        if (e.code && e.message) {
            result[e.code] = e.message;
        }
    }
    return result;
};
export const getRtkErrorTitle = (error: FetchBaseQueryError | SerializedError | undefined, defaultTitle: string = "Ошибка"): string => {
    const apiError = extractRtkError(error);
    if (!apiError) return defaultTitle;

    if (apiError.code) {
        return `${apiError.code} ${apiError.statusCode || ""}`.trim();
    }

    if (apiError.statusCode && apiError.statusCode > 0) {
        return `Error ${apiError.statusCode}`;
    }

    return apiError.code || defaultTitle;
};

export const normalizeUnknownError = (data: any): ApiError => {
    if (!data || typeof data !== "object") {
        return { statusCode: 0, message: "Неизвестная ошибка" };
    }

    const message = data.message || data.title || data.detail || "Нет сообщения";
    const statusCode = data.statusCode || 0;
    const code = data.code;
    const errors = Array.isArray(data.errors) ? data.errors : [];

    return { statusCode, code, message, errors, raw: data };
};
