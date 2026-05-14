import type { FC } from "react";
import { Field, Textarea, Caption1 } from "@fluentui/react-components";
import type { TextareaProps, FieldProps } from "@fluentui/react-components";

interface TextareaWithCounterProps extends Omit<TextareaProps, "onChange"> {
    label: string;
    value: string;
    onChange: (value: string) => void;
    maxLength: number;
    validationMessage?: string | null;
    validationState?: "error" | "none" | "success" | "warning";
    fieldSize?: FieldProps["size"];
}

export const TextareaWithCounter: FC<TextareaWithCounterProps> = ({
    label,
    value,
    onChange,
    maxLength,
    validationMessage,
    validationState,
    placeholder,
    rows = 4,
    fieldSize,
    ...rest
}) => {
    return (
        <Field
            label={label}
            validationMessage={validationMessage}
            validationState={validationState}
            size={fieldSize}
            hint={{
                children: (
                    <div className="flex justify-end">
                        <Caption1 style={{ color: value.length >= maxLength ? "var(--colorPaletteRedForeground1)" : "var(--colorNeutralForeground3)" }}>
                            {value.length}/{maxLength}
                        </Caption1>
                    </div>
                )
            }}
        >
            <Textarea
                value={value}
                onChange={(_e, data) => {
                    if (data.value.length <= maxLength) {
                        onChange(data.value);
                    }
                }}
                placeholder={placeholder}
                rows={rows}
                style={{ width: "100%" }}
                {...rest}
            />
        </Field>
    );
};
