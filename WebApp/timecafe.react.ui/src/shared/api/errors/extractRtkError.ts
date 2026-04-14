import type {SerializedError} from "@reduxjs/toolkit";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import type {ApiError} from "@api/errors/types";

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
                const message = typeof data.message === "string" ? data.message : "Произошла ошибка";
                const code = typeof data.code === "string" ? data.code : undefined;
                const statusCode = typeof data.statusCode === "number" ? data.statusCode : fe.status;

                let errors: ApiError["errors"];
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

                return {statusCode, code, message, errors, raw: fe.data};
            }

            return {statusCode: fe.status, message: `Ошибка сервера (${fe.status})`, raw: fe.data};
        }

        if (fe.status === "FETCH_ERROR") {
            return {statusCode: 0, code: "NetworkError", message: "Ошибка сети. Проверьте соединение.", raw: fe};
        }

        if (fe.status === "PARSING_ERROR") {
            return {statusCode: 0, code: "ParseError", message: "Ошибка обработки ответа сервера", raw: fe};
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
    if (apiError.statusCode === 403) return "Недостаточно прав для выполнения действия.";

    if (apiError.errors?.length) {
        const messages = apiError.errors.map(e => e.message).filter(Boolean);
        if (messages.length) return messages.join(". ");
    }

    return apiError.message;
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
