export const ErrorType = {
    Validation: "Validation",
    NotFound: "NotFound",
    Unauthorized: "Unauthorized",
    Forbidden: "Forbidden",
    Conflict: "Conflict",
    RateLimit: "RateLimit",
    Critical: "Critical",
    BusinessLogic: "BusinessLogic"
} as const;

export type ErrorTypeValue = typeof ErrorType[keyof typeof ErrorType];

export interface ErrorDetail {
    code: string;
    message: string;
    field?: string;
    type: ErrorTypeValue;
    metadata?: Record<string, unknown>;
}

export interface ApiErrorResponse {
    type: string;
    error?: ErrorDetail;
    errors?: ErrorDetail[];
}

export interface HandledError {
    message: string;
    fieldErrors: Record<string, string>;
    shouldShowToast: boolean;
    toastIntent: "error" | "warning" | "info";
    metadata?: Record<string, unknown>;
}
