export const parseErrorMessage = (err: unknown): string => {
    if (!err) return "";

    const asRecord = (value: unknown): Record<string, unknown> | null => {
        if (!value || typeof value !== "object" || Array.isArray(value)) return null;
        return value as Record<string, unknown>;
    };

    if (err instanceof Error) {
        console.log("Обработка ошибки типа: Error");
        return err.message || "Неизвестная ошибка";
    }

    if (Array.isArray(err)) {
        console.log("Обработка ошибки типа: Array");
        const msg = err
            .map((item: unknown) => {
                const obj = asRecord(item);
                const maybeMessage = obj?.message;
                return typeof maybeMessage === "string" ? maybeMessage : String(item);
            })
            .join(" ");
        return msg.includes("[object Object]") ? "Неизвестная ошибка" : msg;
    }


    if (typeof err === "object") {
        console.log("Обработка ошибки типа: Object");
        const errObj = asRecord(err);
        if (!errObj) return "Неизвестная ошибка";

        const errorsObj = asRecord(errObj.errors);
        if (errorsObj) {
            const code = errorsObj.code;
            const description = errorsObj.description;
            if (typeof description === "string") {
                return description;
            }
            if (typeof code === "string") {
                return code;
            }
        }

        const message = errObj.message;
        if (typeof message === "string") {
            return message;
        }

        if (errObj.errors && typeof errObj.errors === "object") {
            const errors = Object.values(errObj.errors as Record<string, unknown>)
                .flatMap((v: unknown) => Array.isArray(v) ? v : [v])
                .filter(Boolean)
                .map(v => String(v))
                .join(", ");
            if (errors) return errors;
        }

        const msg = Object.values(errObj)
            .flatMap(v => Array.isArray(v) ? v : [v])
            .map((v: unknown) => {
                const obj = asRecord(v);
                const maybeMessage = obj?.message;
                return typeof maybeMessage === "string" ? maybeMessage : String(v);
            })
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
    const asRecord = (value: unknown): Record<string, unknown> | null => {
        if (!value || typeof value !== "object" || Array.isArray(value)) return null;
        return value as Record<string, unknown>;
    };

    const errorData = asRecord(error);

    let errorMessage = "";
    let remainingAttempts: number | null = null;
    let attemptsExhausted = false;
    let requiresCaptcha = false;

    if (typeof errorData?.remainingAttempts === "number") {
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

    requiresCaptcha = Boolean(errorData?.requiresCaptcha);

    return {
        errorMessage,
        remainingAttempts,
        attemptsExhausted,
        requiresCaptcha,
    };
};
