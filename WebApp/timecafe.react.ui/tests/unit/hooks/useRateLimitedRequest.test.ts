import {afterEach, beforeEach, describe, expect, it, vi} from "vitest";
import {act, renderHook} from "@testing-library/react";
import {useRateLimitedRequest} from "@hooks/useRateLimitedRequest";

type StorageMock = {
    getItem: (key: string) => string | null;
    setItem: (key: string, value: string) => void;
    removeItem: (key: string) => void;
    clear: () => void;
};

const createStorageMock = (): StorageMock => {
    let store: Record<string, string> = {};

    return {
        getItem: (key) => (key in store ? store[key] : null),
        setItem: (key, value) => {
            store[key] = value;
        },
        removeItem: (key) => {
            delete store[key];
        },
        clear: () => {
            store = {};
        },
    };
};

describe("useRateLimitedRequest", () => {
    let storage: StorageMock;

    beforeEach(() => {
        storage = createStorageMock();
        vi.stubGlobal("localStorage", storage);
        vi.useFakeTimers();
        vi.setSystemTime(new Date("2024-01-01T00:00:00Z"));
    });

    afterEach(() => {
        vi.useRealTimers();
        vi.unstubAllGlobals();
    });

    it("stores retry info and blocks further requests", async () => {
        const fetcher = vi.fn().mockResolvedValue({
            data: {ok: true},
            headers: {retryAfter: 5, remaining: 2},
            status: 200,
        });

        const {result} = renderHook(() => useRateLimitedRequest<{ok: boolean}>("test", fetcher));

        let response: {ok: boolean} | null = null;

        await act(async () => {
            response = await result.current.sendRequest();
        });

        expect(response).toEqual({ok: true});
        expect(result.current.data).toEqual({ok: true});
        expect(result.current.remaining).toBe(2);
        expect(result.current.countdown).toBe(5);
        expect(result.current.isBlocked).toBe(true);
        expect(storage.getItem("rate_limit_test")).not.toBeNull();

        act(() => {
            vi.advanceTimersByTime(5000);
        });

        expect(result.current.countdown).toBe(0);
        expect(result.current.isBlocked).toBe(false);
    });

    it("returns null when request is blocked", async () => {
        const now = Date.now();
        storage.setItem("rate_limit_test", (now + 10000).toString());

        const fetcher = vi.fn().mockResolvedValue({
            data: {ok: true},
            headers: {},
            status: 200,
        });

        const {result} = renderHook(() => useRateLimitedRequest<{ok: boolean}>("test", fetcher));

        let response: {ok: boolean} | null = null;

        await act(async () => {
            response = await result.current.sendRequest();
        });

        expect(response).toBeNull();
        expect(fetcher).not.toHaveBeenCalled();
        expect(result.current.isBlocked).toBe(true);
    });
});
