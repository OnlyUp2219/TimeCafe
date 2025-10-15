import {useNavigate} from "react-router-dom";
import * as React from "react";
import {Button, Field, Input} from "@fluentui/react-components";
import {validateConfirmPassword, validateEmail, validatePassword} from "../../utility/validate.ts";


export const ResetPassword = () => {
    const navigate = useNavigate();

    const [password, setPassword] = React.useState("");
    const [email, setEmail] = React.useState("");
    const [errors, setErrors] = React.useState({
        email: "",
        password: "",
        confirmPassword: "",
    });
    const [confirmPassword, setConfirmPassword] = React.useState("");
    const [isSubmitting, setIsSubmitting] = React.useState(false);

    const validate = () => {
        const emailError = validateEmail(email);
        const passwordError = validatePassword(password);
        const confirmPasswordError = validateConfirmPassword(confirmPassword, password);

        setErrors({email: emailError, password: passwordError, confirmPassword: confirmPasswordError});
        return !emailError && !passwordError && !confirmPasswordError;
    };

    const handleSubmit = () => {
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            navigate("/login");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="auth_card">
            <Field label="Почта"
                   required
                   validationState={errors.email ? "error" : undefined}
                   validationMessage={errors.email}
            >
                <Input
                    value={email}
                    placeholder="Введите почту"
                    type="email"
                    onChange={(_, data) => setEmail(data.value)}
                />
            </Field>

            <Field label="Пароль"
                   required
                   validationState={errors.password ? "error" : undefined}
                   validationMessage={errors.password}>
                <Input
                    value={password}
                    placeholder="Введите пароль"
                    type="password"
                    onChange={(_, data) => setPassword(data.value)}
                />
            </Field>

            <Field
                label="Повторить пароль"
                required
                validationState={errors.confirmPassword ? "error" : undefined}
                validationMessage={errors.confirmPassword}
            >
                <Input
                    type="password"
                    value={confirmPassword}
                    onChange={(_, data) => setConfirmPassword(data.value)}
                    placeholder="Повторите пароль"
                />
            </Field>

            <div className="flex row align-items-center col-12">
                <Button appearance="primary" onClick={handleSubmit} disabled={isSubmitting} type="button">Новый
                    пароль!</Button>

                <Button appearance="primary" onClick={handleSubmit} disabled={isSubmitting} type="button">Новый
                    пароль!</Button>
            </div>


        </div>
    )
}