import {useCallback} from "react";

type Validator<T> = (value: unknown) => value is T;

const safeParseJson = (raw: string): unknown => {
    try {
        return JSON.parse(raw);
    } catch {
        return null;
    }
};

export const useLocalStorageJson = <T,>(key: string, validate?: Validator<T>) => {
    const load = useCallback((): T | null => {
        try {
            const raw = window.localStorage.getItem(key);
            if (!raw) return null;
            const parsed = safeParseJson(raw);
            if (validate) return validate(parsed) ? parsed : null;
            return parsed as T;
        } catch {
            return null;
        }
    }, [key, validate]);

    const save = useCallback(
        (value: T) => {
            try {
                window.localStorage.setItem(key, JSON.stringify(value));
            } catch {
                void 0;
            }
        },
        [key]
    );

    const clear = useCallback(() => {
        try {
            window.localStorage.removeItem(key);
        } catch {
            void 0;
        }
    }, [key]);

    return {load, save, clear};
};
