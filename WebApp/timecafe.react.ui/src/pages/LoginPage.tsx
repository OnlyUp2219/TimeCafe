import {Button, Input, Link, Field, Divider, Subtitle1, Card} from '@fluentui/react-components'
import {useNavigate} from "react-router-dom";
import {validateEmail, validatePassword} from "../utility/validate.ts";
import {loginUser} from "../api/auth.ts";
import {EmailPendingCard} from '../components/EmailPendingCard/EmailPendingCard';
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";
import {useDispatch} from "react-redux";
import {useState} from "react";

export const LoginPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const dispatch = useDispatch();

    const [password, setPassword] = useState("");
    const [email, setEmail] = useState("");
    const [errors, setErrors] = useState({
        email: "",
        password: "",
    });
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [emailPending, setEmailPending] = useState(false);

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
        setErrors({email: emailError, password: passwordError});
        return !emailError && !passwordError;
    };

    const handleSubmit = async () => {

        if (!validate()) return;

        setIsSubmitting(true);
        try {
            const r = await loginUser({email, password}, dispatch);
            if (r.emailNotConfirmed) {
                setEmailPending(true);
                return;
            }
            navigate("/home");
        } catch (err: any) {
            const newErrors = {email: "", password: ""};

            if (Array.isArray(err.errors)) {
                err.errors.forEach((e: { code: string; description: string }) => {
                    const code = e.code.toLowerCase();
                    if (code.includes("email")) newErrors.email += e.description + " ";
                    else if (code.includes("password")) newErrors.password += e.description + " ";
                    else newErrors.email += e.description + " ";
                });
            }
            if (err instanceof Error) {
                const message = err.message;
                showToast(message, "error", "Ошибка");
            }
            setErrors(newErrors);
        } finally {
            setIsSubmitting(false);
        }

    };

    const forgotPasswordSubmit = async () => {
        navigate("/resetPasswordEmail");
    }

    if (emailPending) {
        return <EmailPendingCard onGoToLogin={() => {
            setEmailPending(false);
        }}/>;
    }

    return (
        <Card className="auth_card">
            {ToasterElement}

            <Subtitle1 align={"center"}>Вход</Subtitle1>
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

            <div>
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