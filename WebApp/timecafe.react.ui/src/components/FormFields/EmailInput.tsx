import { Input, Field } from '@fluentui/react-components';
import { useState, useEffect } from 'react';
import { validateEmail as defaultValidateEmail } from '../../utility/validate';

interface EmailInputProps {
    value: string;
    onChange: (value: string) => void;
    disabled?: boolean;
    placeholder?: string;
    validate?: (email: string) => string;
    onValidationChange?: (error: string) => void;
    shouldValidate?: boolean;
}

export const EmailInput = ({
    value,
    onChange,
    disabled = false,
    placeholder = "example@timecafe.ru",
    validate = defaultValidateEmail,
    onValidationChange,
    shouldValidate = true
}: EmailInputProps) => {
    const [error, setError] = useState("");

    useEffect(() => {
        const errorMsg = validate(value);
        setError(errorMsg);
        onValidationChange?.(errorMsg);
    }, [value, validate, onValidationChange]);

    return (
        <Field
            label="Email"
            required
            validationState={shouldValidate && error ? "error" : undefined}
            validationMessage={shouldValidate ? error : undefined}
        >
            <Input
                type="email"
                value={value}
                onChange={(_, data) => onChange(data.value)}
                placeholder={placeholder}
                disabled={disabled}
                className="w-full"
            />
        </Field>
    );
};
