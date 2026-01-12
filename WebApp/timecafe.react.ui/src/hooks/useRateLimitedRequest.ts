import {useState, useEffect, useCallback} from 'react';

export interface RateLimitedHeaders {
    retryAfter?: number;
    windowSeconds?: number;
    remaining?: number;
}

export interface RateLimitedResult<T = unknown> {
    data: T | null;
    countdown: number;
    remaining: number;
    isBlocked: boolean;
    isLoading: boolean;
    sendRequest: () => Promise<T | null>;
}

export function useRateLimitedRequest<T = unknown>(
    id: string,
    fetcher: () => Promise<{ data: T | null; headers: RateLimitedHeaders; status: number }>
): RateLimitedResult<T> {
    const STORAGE_KEY = `rate_limit_${id}`;

    const [data, setData] = useState<T | null>(null);
    const [remaining, setRemaining] = useState<number>(0);
    const [blockUntil, setBlockUntil] = useState<number>(() => {
        const saved = localStorage.getItem(STORAGE_KEY);
        return saved ? parseInt(saved, 10) : 0;
    });
    const [countdown, setCountdown] = useState<number>(() => {
        const saved = localStorage.getItem(STORAGE_KEY);
        if (!saved) return 0;
        const remaining = Math.ceil((parseInt(saved, 10) - Date.now()) / 1000);
        return remaining > 0 ? remaining : 0;
    });
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        const interval = setInterval(() => {
            const now = Date.now();
            if (blockUntil > now) {
                setCountdown(Math.ceil((blockUntil - now) / 1000));
            } else {
                setCountdown(0);
            }
        }, 250);
        return () => clearInterval(interval);
    }, [blockUntil]);

    const sendRequest = useCallback(async (): Promise<T | null> => {
        if (isLoading || blockUntil > Date.now()) return null;

        setIsLoading(true);
        try {
            const {data, headers} = await fetcher();
            setData(data);

            if (headers.remaining !== undefined) {
                setRemaining(headers.remaining);
            }

            if (headers.retryAfter) {
                const until = Date.now() + headers.retryAfter * 1000;
                setBlockUntil(until);
                setCountdown(headers.retryAfter);
                localStorage.setItem(STORAGE_KEY, until.toString());
            }
            
            return data;
        } finally {
            setIsLoading(false);
        }
    }, [isLoading, blockUntil, fetcher, STORAGE_KEY]);

    useEffect(() => {
        if (blockUntil && Date.now() > blockUntil) {
            localStorage.removeItem(STORAGE_KEY);
            setBlockUntil(0);
        }
    }, [blockUntil, STORAGE_KEY]);

    return {
        data,
        countdown,
        remaining,
        isBlocked: blockUntil > Date.now(),
        isLoading,
        sendRequest,
    };
}
