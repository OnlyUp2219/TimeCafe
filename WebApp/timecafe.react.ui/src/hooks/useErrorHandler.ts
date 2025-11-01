import { useState } from "react";
import { ErrorHandler } from "../utility/ErrorHandler";
import type { ToastIntent } from "@fluentui/react-components";


export function useErrorHandler(showToast?: (message: string, intent: ToastIntent, title?: string) => void) {
    const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

    const handleError = (error: unknown) => {
        console.log("🔍 useErrorHandler - handleError called with:", error);
        
        const result = ErrorHandler.handle(error);
        console.log("🔍 useErrorHandler - ErrorHandler.handle result:", result);
        
        setFieldErrors(result.fieldErrors);
        console.log("✅ useErrorHandler - setFieldErrors called with:", result.fieldErrors);

        if (result.shouldShowToast && showToast) {
            console.log("🔔 useErrorHandler - Showing toast:", result.message, result.toastIntent);
            showToast(result.message, ErrorHandler.toToastIntent(result.toastIntent));
        }
        
        return result;
    };

    const clearFieldError = (field: string) => {
        setFieldErrors(prev => {
            const next = { ...prev };
            delete next[field];
            return next;
        });
    };

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
