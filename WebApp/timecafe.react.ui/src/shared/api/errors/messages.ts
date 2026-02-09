import type {ApiError} from "@types";

export const getAuthErrorMessageByStatus = (statusCode: number): string | null => {
    if (statusCode === 401) return "Сессия истекла. Войдите снова.";
    if (statusCode === 403) return "Недостаточно прав для выполнения действия.";
    return null;
};

export const getUserMessage = (error: ApiError): string => {
    const authMsg = getAuthErrorMessageByStatus(error.statusCode);
    if (authMsg) return authMsg;

    if (error.errors && error.errors.length) {
        const msg = error.errors
            .map(e => e.message)
            .map(s => s.trim())
            .filter(Boolean)
            .join(" ");
        if (msg) return msg;
    }

    return error.message || "Произошла ошибка";
};
