import axios from 'axios';
import {withRateLimit, type RateLimitedResponse} from '../utility/rateLimitHelper.ts';

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7057';
const TEST_ENDPOINT_1 = `${API_BASE}/api/test-rate-limit`;
const TEST_ENDPOINT_2 = `${API_BASE}/api/test-rate-limit2`;

export interface RateLimitResponse {
    success: boolean;
    time: string;
}

export async function fetchRateLimited(): Promise<RateLimitedResponse<RateLimitResponse>> {
    return withRateLimit(() => axios.get<RateLimitResponse>(TEST_ENDPOINT_1));
}

export async function fetchRateLimited2(): Promise<RateLimitedResponse<RateLimitResponse>> {
    return withRateLimit(() => axios.get<RateLimitResponse>(TEST_ENDPOINT_2));
}
