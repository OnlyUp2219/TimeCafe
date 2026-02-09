import {Field, Input} from "@fluentui/react-components";
import type {ReactNode} from "react";
import {useEffect, useMemo, useRef, useState} from "react";
import {validatePhoneNumber as defaultValidatePhoneNumber} from "@utility/validate";

interface PhoneInputProps {
    value: string;
    onChange: (value: string) => void;
    disabled?: boolean;
    placeholder?: string;
    label?: string;
    autoComplete?: string;
    required?: boolean;
    validate?: (phoneNumber: string) => string;
    onValidationChange?: (error: string) => void;
    shouldValidate?: boolean;
    validateOnBlur?: boolean;
    trailingElement?: ReactNode;
    externalError?: string;
}

export const PhoneInput = ({
    value,
    onChange,
    disabled = false,
    placeholder = "Введите номер телефона",
    label = "Телефон",
    autoComplete = "tel",
    required = false,
    validate = defaultValidatePhoneNumber,
    onValidationChange,
    shouldValidate = true,
    validateOnBlur = false,
    trailingElement,
    externalError,
}: PhoneInputProps) => {
    const [touched, setTouched] = useState(false);

    const errorMsg = useMemo(() => {
        const trimmed = (value ?? "").trim();
        if (!required && !trimmed) return "";
        return validate(value);
    }, [required, validate, value]);

    const onValidationChangeRef = useRef(onValidationChange);
    useEffect(() => {
        onValidationChangeRef.current = onValidationChange;
    }, [onValidationChange]);

    useEffect(() => {
        onValidationChangeRef.current?.(errorMsg);
    }, [errorMsg]);

    const allowErrorDisplay = shouldValidate && (!validateOnBlur || touched);
    const displayError = externalError || (allowErrorDisplay ? errorMsg : "");

    return (
        <Field
            label={label}
            required={required}
            validationState={shouldValidate && displayError ? "error" : undefined}
            validationMessage={shouldValidate ? displayError : undefined}
        >
            <div className={trailingElement ? "input-with-button" : undefined}>
                <Input
                    type="tel"
                    value={value}
                    onChange={(_, data) => onChange(data.value)}
                    placeholder={placeholder}
                    autoComplete={autoComplete}
                    disabled={disabled}
                    className="w-full"
                    onBlur={() => setTouched(true)}
                />
                {trailingElement}
            </div>
        </Field>
    );
};
