import { useEffect, useRef } from "react";

export const useValidationCallback = (
    onValidationChange: ((error: string) => void) | undefined,
    errorMsg: string
) => {
    const callbackRef = useRef(onValidationChange);

    useEffect(() => {
        callbackRef.current = onValidationChange;
    }, [onValidationChange]);

    useEffect(() => {
        callbackRef.current?.(errorMsg);
    }, [errorMsg]);
};
