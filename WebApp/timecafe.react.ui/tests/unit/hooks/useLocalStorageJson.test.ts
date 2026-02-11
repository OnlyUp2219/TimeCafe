import {afterEach, beforeEach, describe, expect, it, vi} from "vitest";
import {renderHook} from "@testing-library/react";
import {useLocalStorageJson} from "@hooks/useLocalStorageJson";

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

describe("useLocalStorageJson", () => {
    let storage: StorageMock;

    beforeEach(() => {
        storage = createStorageMock();
        vi.stubGlobal("localStorage", storage);
        vi.stubGlobal("window", {localStorage: storage} as Window);
    });

    afterEach(() => {
        vi.unstubAllGlobals();
    });

    it("returns null when storage is empty", () => {
        const {result} = renderHook(() => useLocalStorageJson<{value: number}>("key"));
        expect(result.current.load()).toBeNull();
    });

    it("saves, loads and clears values", () => {
        const {result} = renderHook(() => useLocalStorageJson<{value: number}>("key"));

        result.current.save({value: 5});
        expect(result.current.load()).toEqual({value: 5});

        result.current.clear();
        expect(result.current.load()).toBeNull();
    });

    it("returns null for invalid JSON", () => {
        storage.setItem("key", "{invalid");
        const {result} = renderHook(() => useLocalStorageJson<{value: number}>("key"));

        expect(result.current.load()).toBeNull();
    });

    it("returns null when validator rejects value", () => {
        storage.setItem("key", JSON.stringify({value: "bad"}));
        const validate = (value: unknown): value is {value: number} =>
            typeof value === "object" && value !== null && (value as {value: unknown}).value === 10;

        const {result} = renderHook(() => useLocalStorageJson("key", validate));

        expect(result.current.load()).toBeNull();
    });
});
