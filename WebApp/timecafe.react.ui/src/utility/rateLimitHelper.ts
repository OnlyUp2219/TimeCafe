import axios, {type AxiosResponse} from 'axios';

export interface RateLimitHeaders {
    retryAfter?: number;
    windowSeconds?: number;
    remaining?: number;
}

export interface RateLimitedResponse<T> {
    data: T | null;
    headers: RateLimitHeaders;
    status: number;
}

export async function withRateLimit<T>(
    request: () => Promise<AxiosResponse<T>>
): Promise<RateLimitedResponse<T>> {
    try {
        const res = await request();
        const headers = res.headers || {};
        console.log('Response Headers:', headers);
        const windowSeconds = headers['x-rate-limit-window']
            ? parseInt(headers['x-rate-limit-window'], 10)
            : undefined;
        const remaining = headers['x-rate-limit-remaining']
            ? parseInt(headers['x-rate-limit-remaining'], 10)
            : undefined;
        const retryAfter = headers['retry-after']
            ? parseInt(headers['retry-after'], 10)
            : undefined;

        return {
            data: res.data,
            headers: {windowSeconds, remaining, retryAfter},
            status: res.status
        };
    } catch (error: any) {
        if (axios.isAxiosError(error) && error.response) {
            const headers = error.response.headers || {};
            const retryAfter = headers['retry-after']
                ? parseInt(headers['retry-after'], 10)
                : undefined;
            const remaining = headers['x-rate-limit-remaining']
                ? parseInt(headers['x-rate-limit-remaining'], 10)
                : undefined;

            if (error.response.status === 429) {
                return {
                    data: null,
                    headers: {retryAfter, remaining},
                    status: 429
                };
            }

            throw error.response.data || error;
        }
        throw error;
    }
}
