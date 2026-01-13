import { Input, Field } from '@fluentui/react-components';
import { useState, useEffect } from 'react';
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
    shouldValidate = true
}: ConfirmPasswordInputProps) => {
    const [error, setError] = useState("");

    useEffect(() => {
        const errorMsg = validate(value, passwordValue);
        setError(errorMsg);
        onValidationChange?.(errorMsg);
    }, [value, passwordValue, validate, onValidationChange]);

    return (
        <Field
            label={label}
            required
            validationState={shouldValidate && error ? "error" : undefined}
            validationMessage={shouldValidate ? error : undefined}
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
