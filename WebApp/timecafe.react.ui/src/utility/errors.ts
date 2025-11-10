export const parseErrorMessage = (err: unknown): string => {
    if (!err) return "";

    if (err instanceof Error) {
        console.log("Обработка ошибки типа: Error");
        return err.message || "Неизвестная ошибка";
    }

    if (Array.isArray(err)) {
        console.log("Обработка ошибки типа: Array");
        const msg = err.map(e => (e as any)?.message ?? String(e)).join(" ");
        return msg.includes("[object Object]") ? "Неизвестная ошибка" : msg;
    }


    if (typeof err === "object") {
        console.log("Обработка ошибки типа: Object");
        const errObj = err as Record<string, any>;

        if (errObj.errors && typeof errObj.errors === "object" && !Array.isArray(errObj.errors)) {
            const title = errObj.errors.code;
            const message = errObj.errors.description;
            if (title && message && typeof message === "string") {
                return errObj.errors;
            }
        }

        if (errObj.message && typeof errObj.message === "string") {
            return errObj.message;
        }

        if (errObj.errors && typeof errObj.errors === "object") {
            const errors = Object.values(errObj.errors)
                .flatMap(v => Array.isArray(v) ? v : [v])
                .filter(v => v)
                .join(", ");
            if (errors) return errors;
        }

        const msg = Object.values(errObj)
            .flatMap(v => Array.isArray(v) ? v : [v])
            .map(v => (v as any)?.message ?? String(v))
            .filter(v => v && v !== "[object Object]")
            .join(" ");

        return msg || "Неизвестная ошибка";
    }

    if (typeof err === "string") {
        console.log("Обработка ошибки типа: String");
        return err || "Неизвестная ошибка";
    }

    console.log("Обработка ошибки типа: Unknown");
    return String(err);
};

export interface VerificationErrorResult {
    errorMessage: string;
    remainingAttempts: number | null;
    attemptsExhausted: boolean;
    requiresCaptcha: boolean;
}

export const handleVerificationError = (error: unknown): VerificationErrorResult => {
    const errorData = error as any;

    let errorMessage = "";
    let remainingAttempts: number | null = null;
    let attemptsExhausted = false;
    let requiresCaptcha = false;

    if (errorData?.remainingAttempts !== undefined) {
        remainingAttempts = errorData.remainingAttempts;

        if (errorData.remainingAttempts === 0) {
            attemptsExhausted = true;
            errorMessage = "Превышено количество попыток. Запросите новый код.";
        } else {
            errorMessage = parseErrorMessage(error) || "Неверный код подтверждения";
        }
    } else {
        errorMessage = parseErrorMessage(error) || "Неверный код подтверждения или истек срок действия";
    }

    if (errorData?.requiresCaptcha) {
        requiresCaptcha = true;
    }

    return {
        errorMessage,
        remainingAttempts,
        attemptsExhausted,
        requiresCaptcha,
    };
};
