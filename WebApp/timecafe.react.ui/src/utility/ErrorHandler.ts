import axios from "axios";
import type { ToastIntent } from "@fluentui/react-components";
import { ErrorType, type ErrorDetail, type ApiErrorResponse, type HandledError } from "../types/error";

/**
 * Централизованный обработчик ошибок API
 */
export class ErrorHandler {
    /**
     * Обрабатывает ошибку и возвращает структурированный результат
     */
    static handle(error: unknown): HandledError {
        // Парсим ошибку
        const apiError = this.parseApiError(error);

        // Если это ошибка axios с данными ответа
        if (apiError) {
            return this.handleApiError(apiError);
        }

        // Fallback для неожиданных ошибок
        return {
            message: this.extractGenericMessage(error),
            fieldErrors: {},
            shouldShowToast: true,
            toastIntent: "error"
        };
    }

    /**
     * Парсит ответ API в ApiErrorResponse
     */
    private static parseApiError(error: unknown): ApiErrorResponse | null {
        if (!axios.isAxiosError(error)) return null;

        const data = error.response?.data;
        if (!data || typeof data !== "object") return null;

        // Проверяем структуру ApiErrorResponse
        if ("type" in data && typeof data.type === "string") {
            return data as ApiErrorResponse;
        }

        // Legacy формат - массив ошибок Identity
        if (Array.isArray(data.errors) && data.errors.every((e: unknown) =>
            typeof e === "object" && e !== null && "code" in e && "description" in e
        )) {
            // Преобразуем в новый формат
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

    /**
     * Обрабатывает структурированную ошибку API
     */
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

        // Определяем тип ошибки
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
            case ErrorType.Forbidden:
                // Эти ошибки обычно обрабатываются interceptor'ом
                return {
                    message: primaryError.message || "Доступ запрещен",
                    fieldErrors: {},
                    shouldShowToast: false,
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

    /**
     * Обрабатывает ошибки валидации
     */
    private static handleValidationErrors(errors: ErrorDetail[]): HandledError {
        const fieldErrors: Record<string, string> = {};
        
        errors.forEach(error => {
            if (error.field) {
                fieldErrors[error.field] = error.message;
            }
        });

        return {
            message: "Проверьте введенные данные",
            fieldErrors,
            shouldShowToast: Object.keys(fieldErrors).length === 0, // Toast только если нет полей
            toastIntent: "warning"
        };
    }

    /**
     * Извлекает сообщение из generic ошибки
     */
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

    /**
     * Угадывает поле из кода ошибки (для legacy ошибок)
     */
    private static inferFieldFromCode(code: string): string | undefined {
        const lowerCode = code.toLowerCase();
        
        if (lowerCode.includes("email")) return "email";
        if (lowerCode.includes("password")) return "password";
        if (lowerCode.includes("username") || lowerCode.includes("user")) return "username";
        if (lowerCode.includes("phone")) return "phoneNumber";
        
        return undefined;
    }

    /**
     * Преобразует intent в ToastIntent
     */
    static toToastIntent(intent: "error" | "warning" | "info"): ToastIntent {
        return intent as ToastIntent;
    }
}
