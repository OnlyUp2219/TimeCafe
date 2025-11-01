import axios from "axios";
import type { ToastIntent } from "@fluentui/react-components";
import { ErrorType, type ErrorDetail, type ApiErrorResponse, type HandledError } from "../types/error";

let loggedErrors = new Set<string>();

export class ErrorHandler {
    static handle(error: unknown): HandledError {
        if (Array.isArray(error) && error.length > 0 && typeof error[0] === "object" && "code" in error[0] && "message" in error[0]) {
            const fieldErrors: Record<string, string> = {};
            error.forEach(e => {
                if (e.field) fieldErrors[e.field] = e.message;
            });
            return {
                message: "Проверьте введённые данные",
                fieldErrors,
                shouldShowToast: Object.keys(fieldErrors).length === 0,
                toastIntent: "warning"
            };
        }
        const apiError = this.parseApiError(error);
        if (apiError) {
            return this.handleApiError(apiError);
        }
        return {
            message: this.extractGenericMessage(error),
            fieldErrors: {},
            shouldShowToast: true,
            toastIntent: "error"
        };
    }

    private static parseApiError(error: unknown): ApiErrorResponse | null {
        if (!axios.isAxiosError(error)) return null;
        const data = error.response?.data;
        if (!data || typeof data !== "object") return null;
        const errorTypeMap = [
            "Validation",
            "NotFound",
            "Unauthorized",
            "Forbidden",
            "Conflict",
            "RateLimit",
            "Critical",
            "BusinessLogic"
        ];
        if ("type" in data) {
            let normalizedType = data.type;
            if (typeof normalizedType === "number") {
                normalizedType = errorTypeMap[normalizedType] || "Unknown";
            } else if (typeof normalizedType === "string") {
                normalizedType = normalizedType.replace("Error", "");
            }
            let normalizedErrors = data.errors;
            if (Array.isArray(normalizedErrors)) {
                normalizedErrors = normalizedErrors.map(e => ({
                    ...e,
                    type: typeof e.type === "number" ? errorTypeMap[e.type] : e.type
                }));
            }
            return {
                ...data,
                type: normalizedType,
                errors: normalizedErrors
            } as ApiErrorResponse;
        }
        if (Array.isArray(data.errors) && data.errors.every((e: unknown) =>
            typeof e === "object" && e !== null && "code" in e && "description" in e
        )) {
            return {
                type: "ValidationError",
                errors: (data.errors as Array<{ code: string; description: string }>).map(e => ({
                    code: e.code,
                    message: e.description,
                    type: ErrorType.Validation,
                    field: this.inferFieldFromCode(e.code)
                }))
            };
        }
        return null;
    }

    private static handleApiError(apiError: ApiErrorResponse): HandledError {
        const errors = apiError.errors || (apiError.error ? [apiError.error] : []);
        if (errors.length === 0) {
            return {
                message: "Произошла неизвестная ошибка",
                fieldErrors: {},
                shouldShowToast: true,
                toastIntent: "error"
            };
        }
        const primaryError = errors[0];
        const errorType = primaryError.type;
        switch (errorType) {
            case ErrorType.Validation:
                return this.handleValidationErrors(errors);
            case ErrorType.NotFound:
                return {
                    message: primaryError.message || "Ресурс не найден",
                    fieldErrors: {},
                    shouldShowToast: true,
                    toastIntent: "error"
                };
            case ErrorType.Conflict:
                return {
                    message: primaryError.message || "Конфликт данных",
                    fieldErrors: {},
                    shouldShowToast: true,
                    toastIntent: "warning"
                };
            case ErrorType.RateLimit:
                const remainingSeconds = primaryError.metadata?.remainingSeconds as number | undefined;
                const message = remainingSeconds 
                    ? `Превышен лимит запросов. Подождите ${remainingSeconds} сек.`
                    : primaryError.message || "Превышен лимит запросов";
                return {
                    message,
                    fieldErrors: {},
                    shouldShowToast: true,
                    toastIntent: "warning",
                    metadata: primaryError.metadata
                };
            case ErrorType.BusinessLogic:
                return {
                    message: primaryError.message || "Ошибка бизнес-логики",
                    fieldErrors: {},
                    shouldShowToast: true,
                    toastIntent: "warning"
                };
            case ErrorType.Critical:
                return {
                    message: primaryError.message || "Критическая ошибка сервера",
                    fieldErrors: {},
                    shouldShowToast: true,
                    toastIntent: "error"
                };
            case ErrorType.Unauthorized:
                return {
                    message: primaryError.message || "Необходима авторизация",
                    fieldErrors: {},
                    shouldShowToast: true,
                    toastIntent: "error"
                };
            case ErrorType.Forbidden:
                return {
                    message: primaryError.message || "Доступ запрещен",
                    fieldErrors: {},
                    shouldShowToast: true,
                    toastIntent: "error"
                };
            default:
                return {
                    message: primaryError.message || "Неизвестная ошибка",
                    fieldErrors: {},
                    shouldShowToast: true,
                    toastIntent: "error"
                };
        }
    }

    private static handleValidationErrors(errors: ErrorDetail[]): HandledError {
        const fieldErrors: Record<string, string> = {};
        errors.forEach(error => {
            const key = `${error.code}|${error.message}`;
            if (!loggedErrors.has(key)) {
                console.log("🔍 Validation error:", error);
                loggedErrors.add(key);
            }
            if (error.field) {
                fieldErrors[error.field] = error.message;
            }
        });
        return {
            message: "Проверьте введенные данные",
            fieldErrors,
            shouldShowToast: Object.keys(fieldErrors).length === 0,
            toastIntent: "warning"
        };
    }

    private static extractGenericMessage(error: unknown): string {
        if (error instanceof Error) {
            return error.message || "Неизвестная ошибка";
        }
        if (typeof error === "string") {
            return error;
        }
        if (typeof error === "object" && error !== null && "message" in error) {
            return String((error as {message: unknown}).message);
        }
        return "Неизвестная ошибка";
    }

    private static inferFieldFromCode(code: string): string | undefined {
        const lowerCode = code.toLowerCase();
        if (lowerCode.includes("email")) return "email";
        if (lowerCode.includes("password")) return "password";
        if (lowerCode.includes("username") || lowerCode.includes("user")) return "username";
        if (lowerCode.includes("phone")) return "phoneNumber";
        return undefined;
    }

    static toToastIntent(intent: "error" | "warning" | "info"): ToastIntent {
        return intent as ToastIntent;
    }
}