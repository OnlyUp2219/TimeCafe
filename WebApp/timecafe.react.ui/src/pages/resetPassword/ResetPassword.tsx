import {useLocation, useNavigate} from "react-router-dom";
import {Button, Card, Field, Input, Subtitle1} from "@fluentui/react-components";
import {validateConfirmPassword, validateEmail, validatePassword} from "../../utility/validate.ts";
import {resetPassword} from "../../api/auth.ts";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {useEffect, useState} from "react";
import {useErrorHandler} from "../../hooks/useErrorHandler.ts";

export const ResetPassword = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const {showToast, ToasterElement} = useProgressToast();
    const {fieldErrors, handleError, clearAllErrors} = useErrorHandler(showToast);

    const [email, setEmail] = useState("");
    const [resetCode, setResetCode] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [localErrors, setLocalErrors] = useState({
        email: "",
        password: "",
        confirmPassword: "",
    });

    useEffect(() => {
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

        setLocalErrors({email: emailError, password: passwordError, confirmPassword: confirmPasswordError});
        return !emailError && !passwordError && !confirmPasswordError;
    };

    const handleSubmit = async () => {
        if (!validate()) return;

        clearAllErrors();
        setIsSubmitting(true);
        try {
            await resetPassword({email, resetCode, newPassword: password});
            navigate("/login");
        } catch (error) {
            handleError(error);
        } finally {
            setIsSubmitting(false);
        }
    };

    const allErrors = {...localErrors, ...fieldErrors};


    return (
        <Card className="auth_card">
            {ToasterElement}

            <Subtitle1 align={"center"}>Восстановление пароля!</Subtitle1>

            <Field label="Почта"
                   required
                   validationState={allErrors.email ? "error" : undefined}
                   validationMessage={allErrors.email}
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
                   validationState={allErrors.password ? "error" : undefined}
                   validationMessage={allErrors.password}>
                <Input
                    value={password}
                    placeholder="Введите пароль"
                    type="password"
                    autoComplete="new-password"
                    onChange={(_, data) => setPassword(data.value)}
                />
            </Field>

            <Field
                label="Повторить пароль"
                required
                validationState={allErrors.confirmPassword ? "error" : undefined}
                validationMessage={allErrors.confirmPassword}
            >
                <Input
                    type="password"
                    value={confirmPassword}
                    autoComplete="new-password"
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

        </Card>
    )
}


