import {useLocation, useNavigate} from "react-router-dom";
import * as React from "react";
import {Button, Field, Input, Subtitle1} from "@fluentui/react-components";
import {validateConfirmPassword, validateEmail, validatePassword} from "../../utility/validate.ts";
import {resetPassword} from "../../api/auth.ts";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";


export const ResetPassword = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const {showToast, ToasterElement} = useProgressToast();

    const [email, setEmail] = React.useState("");
    const [resetCode, setResetCode] = React.useState("");
    const [password, setPassword] = React.useState("");
    const [confirmPassword, setConfirmPassword] = React.useState("");
    const [isSubmitting, setIsSubmitting] = React.useState(false);
    const [errors, setErrors] = React.useState({
        email: "",
        password: "",
        confirmPassword: "",
    });

    React.useEffect(() => {
        const params = new URLSearchParams(location.search);
        const emailParam = params.get("email");
        const codeParam = params.get("code");
        if (emailParam) setEmail(emailParam);
        if (codeParam) setResetCode(codeParam);
    }, [location.search]);

    const validate = () => {
        const emailError = validateEmail(email);
        const passwordError = validatePassword(password);
        const confirmPasswordError = validateConfirmPassword(confirmPassword, password);

        setErrors({email: emailError, password: passwordError, confirmPassword: confirmPasswordError});
        return !emailError && !passwordError && !confirmPasswordError;
    };

    const handleSubmit = async () => {
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            await resetPassword({email, resetCode, newPassword: password});
            navigate("/login");
        } catch (err: any) {
            const newErrors = {email: "", password: "", confirmPassword: ""};

            if (Array.isArray(err)) {
                err.forEach((e: { code: string; description: string }) => {
                    const code = e.code.toLowerCase();
                    if (code.includes("email")) newErrors.email += e.description + " ";
                    else if (code.includes("password")) newErrors.password += e.description + " ";
                    else newErrors.email += e.description + " ";
                });
            } else {
                const message = err && typeof err === "object"
                    ? Object.values(err).flat().join(" ")
                    : err instanceof Error
                        ? err.message
                        : String(err);
                showToast(message, "error", "Ошибка");
            }
            setErrors(newErrors);
        } finally {
            setIsSubmitting(false);
        }
    };


    return (
        <div className="auth_card">
            {ToasterElement}

            <Subtitle1 align={"center"}>Восстановление пароля!</Subtitle1>

            <Field label="Почта"
                   required
                   validationState={errors.email ? "error" : undefined}
                   validationMessage={errors.email}
            >
                <Input
                    value={email}
                    type="email"
                    disabled
                />
            </Field>

            <Field label="Код"
                   required
            >
                <Input
                    value={resetCode}
                    disabled
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


            <div className="flex w-full justify-between flex-wrap gap-x-[48px] gap-y-[16px]">
                <Button className="flex-[1]" appearance="secondary" onClick={() => navigate(-1)}
                        disabled={isSubmitting}
                        type="button">Назад</Button>

                <Button className="flex-[1.5]" appearance="primary" onClick={handleSubmit} disabled={isSubmitting}
                        type="button">Восстановить</Button>
            </div>

        </div>
    )
}


