import {Button, Input, Field, Link, Text, Title3, Divider} from '@fluentui/react-components';
import {useState} from "react";
import {useNavigate} from "react-router-dom";
import {validateEmail, validatePassword} from "../utility/validate.ts";
import {registerUser} from "../api/auth.ts";
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";
import {useDispatch} from "react-redux";

export const RegisterPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const dispatch = useDispatch();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [errors, setErrors] = useState({email: "", password: "", confirmPassword: ""});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const apiBase = import.meta.env.VITE_API_BASE_URL;
    const returnUrl = `${window.location.origin}/external-callback`;

    const validate = () => {
        const emailError = validateEmail(email);
        const passwordError = validatePassword(password);
        const confirmPasswordError = password !== confirmPassword ? "Пароли не совпадают" : "";
        setErrors({email: emailError, password: passwordError, confirmPassword: confirmPasswordError});
        return !emailError && !passwordError && !confirmPasswordError;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!validate()) return;

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
                 className="flex flex-col flex-wrap items-center w-full
                 sm:w-auto sm:justify-center sm:p-8">
                <div className="flex flex-col w-full max-w-md gap-[12px]">

                    <div className="flex flex-col items-center">
                        <Title3 block>Создать аккаунт</Title3>
                        <Text block>Присоединитесь к TimeCafe</Text>
                    </div>

                    <Field
                        label="Email"
                        required
                        validationState={errors.email ? "error" : undefined}
                        validationMessage={errors.email}
                    >
                        <Input
                            type="email"
                            value={email}
                            onChange={(_, data) => setEmail(data.value)}
                            placeholder="example@timecafe.ru"
                            disabled={isSubmitting}
                            className="w-full"
                        />
                    </Field>

                    <Field
                        label="Пароль"
                        required
                        validationState={errors.password ? "error" : undefined}
                        validationMessage={errors.password}
                    >
                        <Input
                            type="password"
                            value={password}
                            onChange={(_, data) => setPassword(data.value)}
                            placeholder="Введите пароль"
                            disabled={isSubmitting}
                            className="w-full"
                        />
                    </Field>

                    <Field
                        label="Подтвердите пароль"
                        required
                        validationState={errors.confirmPassword ? "error" : undefined}
                        validationMessage={errors.confirmPassword}
                    >
                        <Input
                            type="password"
                            value={confirmPassword}
                            onChange={(_, data) => setConfirmPassword(data.value)}
                            placeholder="Повторите пароль"
                            disabled={isSubmitting}
                            className="w-full"
                        />
                    </Field>

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
                                appearance="outline"
                                onClick={handleGoogleLogin}
                                disabled={isSubmitting}
                            >
                                <i className="icons8-google"/>
                                Google
                            </Button>

                            <Button
                                appearance="outline"
                                onClick={handleMicrosoftLogin}
                                disabled={isSubmitting}
                            >
                                <i className="icons8-microsoft"/>
                                Microsoft
                            </Button>
                        </div>
                    </div>

                    <div>
                        <Text size={300}>
                            Уже есть аккаунт?{' '}
                            <Link
                                onClick={() => navigate("/login")}
                            >
                                Войти
                            </Link>
                        </Text>
                    </div>
                </div>
            </div>
        </div>
    );
};