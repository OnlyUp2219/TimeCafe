import axios from 'axios';

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7057';
const TEST_ENDPOINT_1 = `${API_BASE}/api/test-rate-limit`;
const TEST_ENDPOINT_2 = `${API_BASE}/api/test-rate-limit2`;

export interface RateLimitResponse {
    success: boolean;
    time: string;
}

export interface RateLimitHeaders {
    retryAfter?: number;
    windowSeconds?: number;
    remaining?: number;
}

export async function fetchRateLimited(): Promise<{
    data: RateLimitResponse | null;
    headers: RateLimitHeaders;
    status: number;
}> {
    try {
        const res = await axios.get<RateLimitResponse>(TEST_ENDPOINT_1);

        const headers = res.headers || {};
        const windowSeconds = parseInt(headers['x-rate-limit-window'] ?? '0', 10);
        const remaining = parseInt(headers['x-rate-limit-remaining'] ?? '0', 10);

        return {
            data: res.data,
            headers: {windowSeconds, remaining},
            status: res.status
        };
    } catch (error: any) {
        if (axios.isAxiosError(error)) {
            const headers = error.response?.headers || {};
            const retryAfter = Number(headers['retry-after'] ?? 0);
            const remaining = Number(headers['x-rate-limit-remaining'] ?? 0);
            return {
                data: null,
                headers: {retryAfter, remaining},
                status: error.response?.status ?? 0
            };
        }
        throw error;
    }
}

export async function fetchRateLimited2(): Promise<{
    data: RateLimitResponse | null;
    headers: RateLimitHeaders;
    status: number;
}> {
    try {
        const res = await axios.get<RateLimitResponse>(TEST_ENDPOINT_2);

        const headers = res.headers || {};
        const windowSeconds = parseInt(headers['x-rate-limit-window'] ?? '0', 10);
        const remaining = parseInt(headers['x-rate-limit-remaining'] ?? '0', 10);

        return {
            data: res.data,
            headers: {windowSeconds, remaining},
            status: res.status
        };
    } catch (error: any) {
        if (axios.isAxiosError(error)) {
            const headers = error.response?.headers || {};
            const retryAfter = Number(headers['retry-after'] ?? 0);
            const remaining = Number(headers['x-rate-limit-remaining'] ?? 0);
            return {
                data: null,
                headers: {retryAfter, remaining},
                status: error.response?.status ?? 0
            };
        }
        throw error;
    }
}
