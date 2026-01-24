import { Input, Field } from '@fluentui/react-components';
import {useEffect, useMemo, useRef} from 'react';
import { validateConfirmPassword as defaultValidateConfirmPassword } from '../../utility/validate';

interface ConfirmPasswordInputProps {
    value: string;
    onChange: (value: string) => void;
    passwordValue: string;
    disabled?: boolean;
    placeholder?: string;
    label?: string;
    validate?: (confirmPassword: string, password: string) => string;
    onValidationChange?: (error: string) => void;
    shouldValidate?: boolean;
    externalError?: string;
}

export const ConfirmPasswordInput = ({
    value,
    onChange,
    passwordValue,
    disabled = false,
    placeholder = "Повторите пароль",
    label = "Повтор пароля",
    validate = defaultValidateConfirmPassword,
    onValidationChange,
    shouldValidate = true,
    externalError,
}: ConfirmPasswordInputProps) => {
    const errorMsg = useMemo(
        () => (shouldValidate ? validate(value, passwordValue) : ""),
        [passwordValue, shouldValidate, validate, value]
    );

    const onValidationChangeRef = useRef(onValidationChange);
    useEffect(() => {
        onValidationChangeRef.current = onValidationChange;
    }, [onValidationChange]);

    useEffect(() => {
        onValidationChangeRef.current?.(errorMsg);
    }, [errorMsg]);

    const displayError = shouldValidate ? (externalError || errorMsg) : "";

    return (
        <Field
            label={label}
            required
            validationState={displayError ? "error" : undefined}
            validationMessage={displayError || undefined}
        >
            <Input
                type="password"
                value={value}
                onChange={(_, data) => onChange(data.value)}
                placeholder={placeholder}
                disabled={disabled}
                className="w-full"
            />
        </Field>
    );
};
