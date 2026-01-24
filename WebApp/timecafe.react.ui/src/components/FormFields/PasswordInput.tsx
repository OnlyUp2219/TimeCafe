import {Input, Field, Caption1} from '@fluentui/react-components';
import {CheckmarkFilled, DismissFilled} from "@fluentui/react-icons";
import {useEffect, useMemo, useRef} from 'react';
import { validatePassword as defaultValidatePassword } from '../../utility/validate';

interface PasswordInputProps {
    value: string;
    onChange: (value: string) => void;
    disabled?: boolean;
    placeholder?: string;
    label?: string;
    showRequirements?: boolean;
    validate?: (password: string) => string;
    onValidationChange?: (error: string) => void;
    shouldValidate?: boolean;
    externalError?: string;
}

const PASSWORD_REQUIREMENTS = [
    {rule: (pwd: string) => pwd.length >= 6, text: "минимум 6 символов"},
    {rule: (pwd: string) => /\d/.test(pwd), text: "хотя бы 1 цифра"},
    {rule: (pwd: string) => /[a-zа-яё]/i.test(pwd), text: "хотя бы 1 буква"}
];

export const PasswordInput = ({
                                  value,
                                  onChange,
                                  disabled = false,
                                  placeholder = "Введите пароль",
                                  label = "Пароль",
                                  showRequirements = false,
                                  validate = defaultValidatePassword,
                                  onValidationChange,
                                  shouldValidate = true,
                                  externalError,
                              }: PasswordInputProps) => {
    const metRequirements = PASSWORD_REQUIREMENTS.map(req => req.rule(value));

    const errorMsg = useMemo(
        () => (shouldValidate ? validate(value) : ""),
        [shouldValidate, validate, value]
    );

    const onValidationChangeRef = useRef(onValidationChange);
    useEffect(() => {
        onValidationChangeRef.current = onValidationChange;
    }, [onValidationChange]);

    useEffect(() => {
        onValidationChangeRef.current?.(errorMsg);
    }, [errorMsg]);

    const displayError = shouldValidate ? (externalError || (!showRequirements ? errorMsg : "")) : "";
    const showFieldError = Boolean(displayError);

    return (
        <>
            <Field
                label={label}
                required
                validationState={showFieldError ? "error" : undefined}
                validationMessage={showFieldError ? displayError : undefined}
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

            {showRequirements && value && (
                <div className="">
                    {PASSWORD_REQUIREMENTS.map((req, idx) => (
                        <Caption1
                            key={idx}
                            className={metRequirements[idx] ? "text-emerald-600" : "text-red-600"}
                        >
                            {metRequirements[idx] ? <CheckmarkFilled/> : <DismissFilled/>} {req.text} <br/>
                        </Caption1>
                    ))}
                </div>
            )}
        </>
    );
};
