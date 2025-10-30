export class RateLimiter {
    private storageKey: string;

    constructor(storageKey: string) {
        this.storageKey = storageKey;
    }

    getSavedRateLimit(): { identifier: string | null; timestamp: number } {
        try {
            const saved = sessionStorage.getItem(this.storageKey);
            if (saved) {
                return JSON.parse(saved);
            }
        } catch (e) {
            console.error(`Error reading rate limit from storage (${this.storageKey})`, e);
        }
        return { identifier: null, timestamp: 0 };
    }

    saveRateLimit(identifier: string, timestamp: number) {
        try {
            sessionStorage.setItem(this.storageKey, JSON.stringify({ identifier, timestamp }));
        } catch (e) {
            console.error(`Error saving rate limit to storage (${this.storageKey})`, e);
        }
    }

    clearRateLimit() {
        try {
            sessionStorage.removeItem(this.storageKey);
        } catch (e) {
            console.error(`Error clearing rate limit from storage (${this.storageKey})`, e);
        }
    }

    getRemainingTime(rateLimitSeconds: number): number {
        const saved = this.getSavedRateLimit();
        if (!saved.timestamp) return 0;
        const elapsed = Math.floor((Date.now() - saved.timestamp) / 1000);
        const remaining = rateLimitSeconds - elapsed;
        return remaining > 0 ? remaining : 0;
    }

    canProceed(identifier: string, rateLimitSeconds: number): boolean {
        const saved = this.getSavedRateLimit();
        
        if (saved.identifier !== identifier) {
            return true;
        }
        
        return this.getRemainingTime(rateLimitSeconds) === 0;
    }
}
