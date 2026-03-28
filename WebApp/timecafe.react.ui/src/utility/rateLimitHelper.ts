import axios, {type AxiosResponse} from "axios";

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

const parseHeader = (headers: Record<string, string>, key: string): number | undefined => {
    const value = headers[key];
    return value ? Number.parseInt(value, 10) : undefined;
};

export async function withRateLimit<T>(
    request: () => Promise<AxiosResponse<T>>
): Promise<RateLimitedResponse<T>> {
    try {
        const res = await request();
        const headers = res.headers || {};

        return {
            data: res.data,
            headers: {
                windowSeconds: parseHeader(headers, "x-rate-limit-window"),
                remaining: parseHeader(headers, "x-rate-limit-remaining"),
                retryAfter: parseHeader(headers, "retry-after"),
            },
            status: res.status
        };
    } catch (error: unknown) {
        if (axios.isAxiosError(error) && error.response) {
            const headers = error.response.headers || {};
            const retryAfter = parseHeader(headers, "retry-after");
            const remaining = parseHeader(headers, "x-rate-limit-remaining");

            if (error.response.status === 429) {
                return {
                    data: null,
                    headers: {retryAfter, remaining},
                    status: 429
                };
            }

            throw error.response.data ?? error;
        }
        throw error;
    }
}
