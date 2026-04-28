import type {ApiError, ApiErrorItem} from "@api/errors/types";

export const getAuthErrorMessageByStatus = (statusCode: number): string | null => {
    if (statusCode === 401) return "Сессия истекла. Войдите снова.";
    if (statusCode === 403) return "Недостаточно прав для выполнения действия.";
    return null;
};

export const getUserMessage = (error: ApiError): string => {
    const authMsg = getAuthErrorMessageByStatus(error.statusCode);
    if (authMsg && !error.code && (!error.errors || error.errors.length === 0)) return authMsg;

    let description = error.message || "Произошла ошибка";

    if (error.errors && error.errors.length) {
        const msg = error.errors
            .map((e: ApiErrorItem) => e.message)
            .map((s: string) => s.trim())
            .filter(Boolean)
            .join(" ");
        if (msg) description = msg;
    }

    const code = error.code || (error.errors && error.errors.length > 0 ? error.errors[0].code : null) || "Error";
    const statusPart = error.statusCode ? ` (${error.statusCode})` : "";

    return `${code}${statusPart}: ${description}`;
};
