import {Button, Link, Body2, Caption1, Title3, Divider} from '@fluentui/react-components';
import {useState, useCallback} from "react";
import {useNavigate} from "react-router-dom";
import {loginUser} from "../../api/auth.ts";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {EmailInput, PasswordInput} from "../../components/FormFields";
import {useDispatch} from "react-redux";
import {authFormContainerClassName} from "../../layouts/authLayout";

export const LoginPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const dispatch = useDispatch();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [errors, setErrors] = useState({email: "", password: ""});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);

    const apiBase = import.meta.env.VITE_API_BASE_URL;
    const returnUrl = `${window.location.origin}/external-callback`;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(true);
        if (errors.email || errors.password || !email || !password) return;

        setIsSubmitting(true);
        try {
            const r = await loginUser({email, password}, dispatch);
            if (r.emailNotConfirmed) {
                showToast("Подтвердите email для входа", "warning");
                return;
            }
            navigate("/home");
        } catch (err: unknown) {
            if (err && typeof err === 'object' && 'errors' in err && Array.isArray((err as {
                errors: Array<{ description: string }>
            }).errors)) {
                const message = (err as {
                    errors: Array<{ description: string }>
                }).errors.map(e => e.description).join(" ");
                showToast(message, "error");
            } else {
                showToast("Ошибка входа. Проверьте данные", "error");
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleGoogleLogin = () => {
        window.location.href = `${apiBase}/authenticate/login/google?returnUrl=${encodeURIComponent(returnUrl)}`;
    };

    const handleMicrosoftLogin = () => {
        window.location.href = `${apiBase}/authenticate/login/microsoft?returnUrl=${encodeURIComponent(returnUrl)}`;
    };

    const handleEmailValidationChange = useCallback((error: string) => {
        setErrors(prev => ({...prev, email: error}));
    }, []);

    const handlePasswordValidationChange = useCallback((error: string) => {
        setErrors(prev => ({...prev, password: error}));
    }, []);

    return (
        <div
            className="!grid grid-cols-1 items-center justify-center
             sm:grid-cols-2 sm:justify-stretch sm:items-stretch ">
            {ToasterElement}

            {/* Login Form */}
            <div id="Form"
                 className={authFormContainerClassName}>
                <form onSubmit={handleSubmit} className="flex flex-col w-full max-w-md gap-[12px]">

                    <div className="flex flex-col items-center">
                        <Title3 block>Добро пожаловать</Title3>
                        <Body2 block>Войдите в свой аккаунт TimeCafe</Body2>
                    </div>

                    <EmailInput
                        value={email}
                        onChange={setEmail}
                        disabled={isSubmitting}
                        onValidationChange={handleEmailValidationChange}
                        shouldValidate={submitted}
                    />

                    <PasswordInput
                        value={password}
                        onChange={setPassword}
                        disabled={isSubmitting}
                        onValidationChange={handlePasswordValidationChange}
                        shouldValidate={submitted}
                    />

                    <div className="flex items-center justify-between">
                        <Link
                            onClick={() => navigate("/reset-password")}
                            className=""
                        >
                            Забыли пароль?
                        </Link>
                    </div>

                    <Button
                        appearance="primary"
                        type="submit"
                        disabled={isSubmitting}
                        className="sm:w-full"
                    >
                        {isSubmitting ? "Вход..." : "Войти"}
                    </Button>

                    <Divider appearance="brand" className="divider grow-0">или продолжить с</Divider>

                    <div>
                        <div className="grid grid-cols-1 gap-[12px] sm:grid-cols-2">
                            <Button
                                appearance="secondary"
                                onClick={handleGoogleLogin}
                                disabled={isSubmitting}
                            >
                                <i className="icons8-google"/>
                                Google
                            </Button>

                            <Button
                                appearance="secondary"
                                onClick={handleMicrosoftLogin}
                                disabled={isSubmitting}
                            >
                                <i className="icons8-microsoft"/>
                                Microsoft
                            </Button>
                        </div>
                    </div>

                    <div>
                        <Caption1>
                            Нет аккаунта?{' '}
                            <Link
                                onClick={() => navigate("/register")}
                            >
                                Зарегистрироваться
                            </Link>
                        </Caption1>
                    </div>
                </form>
            </div>

            {/* Hero Section - Right Side (Desktop Only) */}
            <div id="Right Side" className="hidden sm:block bg-sky-400">
            </div>
        </div>
    );
};
