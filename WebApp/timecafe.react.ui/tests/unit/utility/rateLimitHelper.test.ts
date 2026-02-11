import {afterEach, beforeEach, describe, expect, it, vi} from "vitest";
import type {AxiosResponse, InternalAxiosRequestConfig} from "axios";
import {AxiosError} from "axios";
import {withRateLimit} from "@utility/rateLimitHelper";

const createResponse = <T,>(
    data: T,
    status: number,
    headers: Record<string, string>,
    statusText = "OK"
): AxiosResponse<T> => ({
    data,
    status,
    statusText,
    headers,
    config: {headers: {}} as InternalAxiosRequestConfig,
});

describe("withRateLimit", () => {
    let logSpy: ReturnType<typeof vi.spyOn>;

    beforeEach(() => {
        logSpy = vi.spyOn(console, "log").mockImplementation(() => {});
    });

    afterEach(() => {
        logSpy.mockRestore();
    });

    it("parses rate limit headers on success", async () => {
        const response = createResponse(
            {ok: true},
            200,
            {
                "x-rate-limit-window": "60",
                "x-rate-limit-remaining": "2",
                "retry-after": "3",
            }
        );

        const result = await withRateLimit(() => Promise.resolve(response));

        expect(result.status).toBe(200);
        expect(result.data).toEqual({ok: true});
        expect(result.headers).toEqual({windowSeconds: 60, remaining: 2, retryAfter: 3});
    });

    it("returns retry info for 429 responses", async () => {
        const response = createResponse(
            {message: "Too Many"},
            429,
            {
                "retry-after": "5",
                "x-rate-limit-remaining": "0",
            },
            "Too Many Requests"
        );

        const error = new AxiosError("Too Many", undefined, undefined, undefined, response);
        const result = await withRateLimit(() => Promise.reject(error));

        expect(result.status).toBe(429);
        expect(result.data).toBeNull();
        expect(result.headers).toEqual({retryAfter: 5, remaining: 0});
    });

    it("throws response data for non-429 errors", async () => {
        const response = createResponse("boom", 500, {}, "Server Error");

        const error = new AxiosError("Server", undefined, undefined, undefined, response);

        await expect(withRateLimit(() => Promise.reject(error))).rejects.toBe("boom");
    });
});
