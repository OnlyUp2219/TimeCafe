import {getUserMessageFromUnknown} from "@api/errors/getUserMessageFromUnknown";

export interface VerificationErrorResult {
    errorMessage: string;
    remainingAttempts: number | null;
    attemptsExhausted: boolean;
    requiresCaptcha: boolean;
}

export const handleVerificationError = (error: unknown): VerificationErrorResult => {
    const asRecord = (value: unknown): Record<string, unknown> | null => {
        if (!value || typeof value !== "object" || Array.isArray(value)) return null;
        return value as Record<string, unknown>;
    };

    const errorData = asRecord(error);

    const remainingAttempts: number | null =
        typeof errorData?.remainingAttempts === "number" ? errorData.remainingAttempts : null;

    const requiresCaptcha = Boolean(errorData?.requiresCaptcha);

    if (remainingAttempts === 0) {
        return {
            errorMessage: "Превышено количество попыток. Запросите новый код.",
            remainingAttempts,
            attemptsExhausted: true,
            requiresCaptcha,
        };
    }

    const errorMessage = getUserMessageFromUnknown(error) || "Неверный код подтверждения или истек срок действия";

    return {
        errorMessage,
        remainingAttempts,
        attemptsExhausted: false,
        requiresCaptcha,
    };
};
