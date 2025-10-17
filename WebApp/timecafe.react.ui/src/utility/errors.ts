export const parseErrorMessage = (err: unknown): string => {
    if (!err) return "";

    if (Array.isArray(err)) {
        const msg = err.map(e => (e as any)?.message ?? String(e)).join(" ");
        return msg.includes("[object Object]") ? "Неизвестная ошибка" : msg;
    }

    if (typeof err === "object") {
        const msg = Object.values(err as Record<string, any>)
            .flatMap(v => Array.isArray(v) ? v : [v])
            .map(v => (v as any)?.message ?? String(v))
            .join(" ");
        return msg.includes("[object Object]") || !msg ? "Неизвестная ошибка" : msg;
    }

    if (err instanceof Error) return err.message;

    return String(err);
};
