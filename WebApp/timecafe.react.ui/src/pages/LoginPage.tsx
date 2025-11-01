import {Button, Input, Link, Field, Divider, Subtitle1, Card} from '@fluentui/react-components'
import {useNavigate} from "react-router-dom";
import {validateEmail, validatePassword} from "../utility/validate.ts";
import {loginUser} from "../api/auth.ts";
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";
import {useDispatch, useSelector} from "react-redux";
import type {RootState} from "../store";
import {useState} from "react";
import {useErrorHandler} from "../hooks/useErrorHandler.ts";

export const LoginPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const {fieldErrors, handleError, clearAllErrors} = useErrorHandler(showToast);
    const dispatch = useDispatch();
    const refreshToken = useSelector((state: RootState) => state.auth.refreshToken);
    const accessToken = useSelector((state: RootState) => state.auth.accessToken);

    const [password, setPassword] = useState("");
    const [email, setEmail] = useState("");
    const [localErrors, setLocalErrors] = useState({
        email: "",
        password: "",
    });
    const [isSubmitting, setIsSubmitting] = useState(false);

    const apiBase = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7057";
    const returnUrl = `${window.location.origin}/external-callback`;

    const handleGoogleLogin = () => {
        window.location.href = `${apiBase}/authenticate/login/google?returnUrl=${encodeURIComponent(returnUrl)}`;
    };

    const handleMicrosoftLogin = () => {
        window.location.href = `${apiBase}/authenticate/login/microsoft?returnUrl=${encodeURIComponent(returnUrl)}`;
    };

    const validate = () => {
        const emailError = validateEmail(email);
        const passwordError = validatePassword(password);
        setLocalErrors({email: emailError, password: passwordError});
        return !emailError && !passwordError;
    };

    const handleSubmit = async () => {

        if (!validate()) return;

        clearAllErrors();
        setIsSubmitting(true);
        try {
            await loginUser({email, password}, dispatch);
            navigate("/home");
        } catch (error) {
            handleError(error);
        } finally {
            setIsSubmitting(false);
            console.log("accessToken:", accessToken);
            console.log("refreshToken:", refreshToken);
        }

    };

    const forgotPasswordSubmit = async () => {
        navigate("/resetPasswordEmail");
    }

    const allErrors = {...localErrors, ...fieldErrors};

    return (
        <Card className="auth_card">
            {ToasterElement}

            <Subtitle1 align={"center"}>Вход</Subtitle1>
            <Field label="Почта"
                   required
                   validationState={allErrors.email ? "error" : undefined}
                   validationMessage={allErrors.email}
            >
                <Input
                    value={email}
                    placeholder="Введите почту"
                    type="email"
                    onChange={(_, data) => setEmail(data.value)}
                />
            </Field>

            <div>
                <Field label="Пароль"
                       required
                       validationState={allErrors.password ? "error" : undefined}
                       validationMessage={allErrors.password}>
                    <Input
                        value={password}
                        placeholder="Введите пароль"
                        type="password"
                        onChange={(_, data) => setPassword(data.value)}
                    />
                </Field>

                <Link onClick={forgotPasswordSubmit}>Забыли пароль?</Link>
            </div>

            <Button appearance="primary" onClick={handleSubmit} disabled={isSubmitting} type="button">Войти</Button>

            <Link onClick={() => navigate("/sign")}>Зарегистрироваться</Link>

            <Divider appearance="brand" className="divider">или</Divider>

            <Button icon={<div className="icons8-google"></div>} appearance="outline"
                    onClick={handleGoogleLogin}>Google</Button>
            <Button icon={<div className="icons8-microsoft"></div>} appearance="outline"
                    onClick={handleMicrosoftLogin}>Microsoft</Button>
        </Card>
    )
}