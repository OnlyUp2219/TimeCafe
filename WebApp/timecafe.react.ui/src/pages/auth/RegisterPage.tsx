import {Button, Link, Body2, Caption1, Title3, Divider} from '@fluentui/react-components';
import {useState, useCallback} from "react";
import {useNavigate} from "react-router-dom";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {EmailInput, PasswordInput, ConfirmPasswordInput} from "../../components/FormFields";
import {authFormContainerClassName} from "../../layouts/authLayout";

export const RegisterPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [errors, setErrors] = useState({email: "", password: "", confirmPassword: ""});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);


    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(true);
        if (errors.email || errors.password || errors.confirmPassword || !email || !password) return;

        setIsSubmitting(true);
        try {
            // TODO: STUB - реальная регистрация (подключить бек)
            // await registerUser({email, password}, dispatch);
            showToast("Проверьте почту для подтверждения аккаунта", "success");
            navigate("/login");
        } catch (err: unknown) {
            if (err && typeof err === 'object' && 'errors' in err && Array.isArray((err as {
                errors: Array<{ description: string }>
            }).errors)) {
                const message = (err as {
                    errors: Array<{ description: string }>
                }).errors.map(e => e.description).join(" ");
                showToast(message, "error");
            } else {
                showToast("Ошибка регистрации. Попробуйте позже", "error");
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleGoogleLogin = () => {
        // TODO: STUB - интеграция Google OAuth (подключить бек)
        // window.location.href = `${apiBase}/authenticate/login/google?returnUrl=${encodeURIComponent(returnUrl)}`;
        showToast("Google OAuth еще не интегрирован", "warning");
    };

    const handleMicrosoftLogin = () => {
        // TODO: STUB - интеграция Microsoft OAuth (подключить бек)
        // window.location.href = `${apiBase}/authenticate/login/microsoft?returnUrl=${encodeURIComponent(returnUrl)}`;
        showToast("Microsoft OAuth еще не интегрирован", "warning");
    };

    const handleEmailValidationChange = useCallback((error: string) => {
        setErrors(prev => ({...prev, email: error}));
    }, []);

    const handlePasswordValidationChange = useCallback((error: string) => {
        setErrors(prev => ({...prev, password: error}));
    }, []);

    const handleConfirmPasswordValidationChange = useCallback((error: string) => {
        setErrors(prev => ({...prev, confirmPassword: error}));
    }, []);

    return (
        <div
            className="!grid grid-cols-1 items-center justify-center
             sm:grid-cols-2 sm:justify-stretch sm:items-stretch ">
            {ToasterElement}

            {/* Hero Section - Left Side (Desktop Only) */}
            <div id="Left Side" className="hidden sm:block bg-sky-400">
            </div>

            {/* Register Form */}
            <div id="Form"
                 className={authFormContainerClassName}>
                <div className="flex flex-col w-full max-w-md gap-[12px]">

                    <div className="flex flex-col items-center">
                        <Title3 block>Создать аккаунт</Title3>
                        <Body2 block>Присоединитесь к TimeCafe</Body2>
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
                        showRequirements={true}
                        onValidationChange={handlePasswordValidationChange}
                        shouldValidate={submitted}
                    />

                    <ConfirmPasswordInput
                        value={confirmPassword}
                        onChange={setConfirmPassword}
                        passwordValue={password}
                        disabled={isSubmitting}
                        onValidationChange={handleConfirmPasswordValidationChange}
                        shouldValidate={submitted}
                    />

                    <Button
                        appearance="primary"
                        onClick={handleSubmit}
                        disabled={isSubmitting}
                        className="sm:w-full"
                    >
                        {isSubmitting ? "Регистрация..." : "Зарегистрироваться"}
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
                            Уже есть аккаунт?{' '}
                            <Link
                                onClick={() => navigate("/login")}
                            >
                                Войти
                            </Link>
                        </Caption1>
                    </div>
                </div>
            </div>
        </div>
    );
};