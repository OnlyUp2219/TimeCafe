import {Input, Field} from "@fluentui/react-components";
import {useMemo} from "react";
import {validateEmail as defaultValidateEmail} from "@utility/validate";
import {useValidationCallback} from "@hooks/useValidationCallback";
import type {ReactNode} from "react";

interface EmailInputProps {
    value: string;
    onChange: (value: string) => void;
    disabled?: boolean;
    placeholder?: string;
    autoComplete?: string;
    validate?: (email: string) => string;
    onValidationChange?: (error: string) => void;
    shouldValidate?: boolean;
    trailingElement?: ReactNode;
    externalError?: string;
    size?: "small" | "medium" | "large";
}

export const EmailInput = ({
    value,
    onChange,
    disabled = false,
    placeholder = "example@timecafe.ru",
    autoComplete = "email",
    validate = defaultValidateEmail,
    onValidationChange,
    shouldValidate = true,
    trailingElement,
    externalError,
    size,
}: EmailInputProps) => {
    const errorMsg = useMemo(() => validate(value), [validate, value]);

    useValidationCallback(onValidationChange, errorMsg);

    const displayError = externalError || errorMsg;

    return (
        <Field
            label="Email"
            required
            size={size}
            validationState={shouldValidate && displayError ? "error" : undefined}
            validationMessage={shouldValidate ? displayError : undefined}
        >
            <div className="input-with-button">
                <Input
                    type="email"
                    value={value}
                    onChange={(_, data) => onChange(data.value)}
                    placeholder={placeholder}
                    autoComplete={autoComplete}
                    disabled={disabled}
                    size={size}
                    className="w-full"
                />
                {trailingElement}
            </div>
        </Field>
    );
};
