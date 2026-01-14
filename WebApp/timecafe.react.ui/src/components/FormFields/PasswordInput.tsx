import {Input, Field, Caption1} from '@fluentui/react-components';
import {CheckmarkFilled, DismissFilled} from "@fluentui/react-icons";
import { useState, useEffect } from 'react';
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
                                  shouldValidate = true
                              }: PasswordInputProps) => {
    const [error, setError] = useState("");
    const metRequirements = PASSWORD_REQUIREMENTS.map(req => req.rule(value));

    useEffect(() => {
        const errorMsg = validate(value);
        setError(errorMsg);
        onValidationChange?.(errorMsg);
    }, [value, validate, onValidationChange]);

    return (
        <>
            <Field
                label={label}
                required
                validationState={shouldValidate && error && !showRequirements ? "error" : undefined}
                validationMessage={shouldValidate && !showRequirements ? error : undefined}
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
