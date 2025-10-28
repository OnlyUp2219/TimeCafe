const STORAGE_KEY = "sms_rate_limit";

export const getSavedRateLimit = (): { phoneNumber: string | null; timestamp: number } => {
    try {
        const saved = sessionStorage.getItem(STORAGE_KEY);
        if (saved) {
            return JSON.parse(saved);
        }
    } catch (e) {
        console.error("Error reading rate limit from storage", e);
    }
    return { phoneNumber: null, timestamp: 0 };
};

export const saveRateLimit = (phoneNumber: string, timestamp: number) => {
    try {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify({ phoneNumber, timestamp }));
    } catch (e) {
        console.error("Error saving rate limit to storage", e);
    }
};

export const clearRateLimit = () => {
    try {
        sessionStorage.removeItem(STORAGE_KEY);
    } catch (e) {
        console.error("Error clearing rate limit from storage", e);
    }
};

export const getRemainingTime = (rateLimitSeconds: number): number => {
    const saved = getSavedRateLimit();
    if (!saved.timestamp) return 0;
    const elapsed = Math.floor((Date.now() - saved.timestamp) / 1000);
    const remaining = rateLimitSeconds - elapsed;
    return remaining > 0 ? remaining : 0;
};
