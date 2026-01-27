import { Input, Field } from '@fluentui/react-components';
import {useEffect, useMemo, useRef} from 'react';
import { validateEmail as defaultValidateEmail } from '../../utility/validate';
import type { ReactNode } from 'react';

interface EmailInputProps {
    value: string;
    onChange: (value: string) => void;
    disabled?: boolean;
    placeholder?: string;
    validate?: (email: string) => string;
    onValidationChange?: (error: string) => void;
    shouldValidate?: boolean;
    trailingElement?: ReactNode;
    externalError?: string;
}

export const EmailInput = ({
    value,
    onChange,
    disabled = false,
    placeholder = "example@timecafe.ru",
    validate = defaultValidateEmail,
    onValidationChange,
    shouldValidate = true,
    trailingElement,
    externalError,
}: EmailInputProps) => {
    const errorMsg = useMemo(() => validate(value), [validate, value]);

    const onValidationChangeRef = useRef(onValidationChange);
    useEffect(() => {
        onValidationChangeRef.current = onValidationChange;
    }, [onValidationChange]);

    useEffect(() => {
        onValidationChangeRef.current?.(errorMsg);
    }, [errorMsg]);

    const displayError = externalError || errorMsg;

    return (
        <Field
            label="Email"
            required
            validationState={shouldValidate && displayError ? "error" : undefined}
            validationMessage={shouldValidate ? displayError : undefined}
        >
            <div className="input-with-button">
                <Input
                    type="email"
                    value={value}
                    onChange={(_, data) => onChange(data.value)}
                    placeholder={placeholder}
                    disabled={disabled}
                    className="w-full"
                />
                {trailingElement}
            </div>
        </Field>
    );
};
