import {Input, Field, Caption1} from "@fluentui/react-components";
import {CheckmarkFilled, DismissFilled} from "@fluentui/react-icons";
import {useMemo} from "react";
import {validatePassword as defaultValidatePassword, PASSWORD_RULES} from "@utility/validate";
import {useValidationCallback} from "@hooks/useValidationCallback";

interface PasswordInputProps {
    value: string;
    onChange: (value: string) => void;
    disabled?: boolean;
    placeholder?: string;
    label?: string;
    autoComplete?: string;
    showRequirements?: boolean;
    validate?: (password: string) => string;
    onValidationChange?: (error: string) => void;
    shouldValidate?: boolean;
    externalError?: string;
    size?: "small" | "medium" | "large";
}

const PASSWORD_REQUIREMENTS = [
    {rule: PASSWORD_RULES.minLength, text: "минимум 6 символов"},
    {rule: PASSWORD_RULES.hasDigit, text: "хотя бы 1 цифра"},
    {rule: PASSWORD_RULES.hasLetter, text: "хотя бы 1 буква"}
];

export const PasswordInput = ({
                                  value,
                                  onChange,
                                  disabled = false,
                                  placeholder = "Введите пароль",
                                  label = "Пароль",
                                  autoComplete = "current-password",
                                  showRequirements = false,
                                  validate = defaultValidatePassword,
                                  onValidationChange,
                                  shouldValidate = true,
                                  externalError,
                                  size,
                              }: PasswordInputProps) => {
    const metRequirements = PASSWORD_REQUIREMENTS.map(req => req.rule(value));

    const errorMsg = useMemo(
        () => (shouldValidate ? validate(value) : ""),
        [shouldValidate, validate, value]
    );

    useValidationCallback(onValidationChange, errorMsg);

    const displayError = shouldValidate ? (externalError || (!showRequirements ? errorMsg : "")) : "";
    const showFieldError = Boolean(displayError);

    return (
        <>
            <Field
                label={label}
                required
                size={size}
                validationState={showFieldError ? "error" : undefined}
                validationMessage={showFieldError ? displayError : undefined}
            >
                <Input
                    type="password"
                    value={value}
                    onChange={(_, data) => onChange(data.value)}
                    placeholder={placeholder}
                    autoComplete={autoComplete}
                    disabled={disabled}
                    size={size}
                    className="w-full"
                />
            </Field>

            {showRequirements && value && (
                <div className="">
                    {PASSWORD_REQUIREMENTS.map((req, idx) => (
                        <Caption1
                            key={idx}
                            className={metRequirements[idx] ? "text-(--colorPaletteGreenForeground1)" : "text-(--colorPaletteRedForeground1)"}
                        >
                            {metRequirements[idx] ? <CheckmarkFilled/> : <DismissFilled/>} {req.text} <br/>
                        </Caption1>
                    ))}
                </div>
            )}
        </>
    );
};
