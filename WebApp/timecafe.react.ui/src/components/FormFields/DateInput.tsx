import {Field, Input} from "@fluentui/react-components";
import {useEffect, useMemo, useRef} from "react";

interface DateInputProps {
    value?: Date;
    onChange: (value: Date | undefined) => void;
    disabled?: boolean;
    label?: string;
    placeholder?: string;
    validate?: (value: Date | undefined) => string;
    onValidationChange?: (error: string) => void;
    shouldValidate?: boolean;
    required?: boolean;
}

const formatDateForInput = (date: Date): string => {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, "0");
    const dd = String(date.getDate()).padStart(2, "0");
    return `${yyyy}-${mm}-${dd}`;
};

export const DateInput = ({
                              value,
                              onChange,
                              disabled = false,
                              label = "Дата",
                              placeholder,
                              validate,
                              onValidationChange,
                              shouldValidate = true,
                              required = false,
                          }: DateInputProps) => {
    const inputValue = useMemo(() => (value ? formatDateForInput(value) : ""), [value]);

        const errorMsg = useMemo(() => (validate ? validate(value) : ""), [validate, value]);

        const onValidationChangeRef = useRef(onValidationChange);
        useEffect(() => {
            onValidationChangeRef.current = onValidationChange;
        }, [onValidationChange]);

        useEffect(() => {
            onValidationChangeRef.current?.(errorMsg);
        }, [errorMsg]);

    return (
        <Field
            label={label}
            required={required}
            validationState={shouldValidate && errorMsg ? "error" : undefined}
            validationMessage={shouldValidate ? errorMsg : undefined}
        >
            <Input
                type="date"
                value={inputValue}
                onChange={(_, data) => {
                    const next = data.value ? new Date(`${data.value}T00:00:00`) : undefined;
                    onChange(next);
                }}
                placeholder={placeholder}
                disabled={disabled}
                className="w-full"
            />
        </Field>
    );
};
