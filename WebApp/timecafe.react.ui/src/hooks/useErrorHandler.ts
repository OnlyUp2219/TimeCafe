import { useState } from "react";
import { ErrorHandler } from "../utility/ErrorHandler";
import type { ToastIntent } from "@fluentui/react-components";

/**
 * React hook для обработки ошибок с поддержкой toast и валидации полей
 */
export function useErrorHandler(showToast?: (message: string, intent: ToastIntent, title?: string) => void) {
    const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

    /**
     * Обрабатывает ошибку и автоматически показывает toast если нужно
     */
    const handleError = (error: unknown) => {
        const result = ErrorHandler.handle(error);
        
        // Устанавливаем ошибки полей
        setFieldErrors(result.fieldErrors);
        
        // Показываем toast если нужно
        if (result.shouldShowToast && showToast) {
            showToast(result.message, ErrorHandler.toToastIntent(result.toastIntent));
        }
        
        return result;
    };

    /**
     * Очищает ошибку конкретного поля
     */
    const clearFieldError = (field: string) => {
        setFieldErrors(prev => {
            const next = { ...prev };
            delete next[field];
            return next;
        });
    };

    /**
     * Очищает все ошибки полей
     */
    const clearAllErrors = () => {
        setFieldErrors({});
    };

    return {
        fieldErrors,
        handleError,
        clearFieldError,
        clearAllErrors,
        setFieldErrors
    };
}
