import {Button, Input, Field, Card, Link, Text, Title3, Divider} from '@fluentui/react-components';
import {useState} from "react";
import {useNavigate} from "react-router-dom";
import {validateEmail, validatePassword} from "../utility/validate.ts";
import {loginUser} from "../api/auth.ts";
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";
import {useDispatch} from "react-redux";

export const LoginPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const dispatch = useDispatch();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [errors, setErrors] = useState({email: "", password: ""});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const apiBase = import.meta.env.VITE_API_BASE_URL;
    const returnUrl = `${window.location.origin}/external-callback`;

    const validate = () => {
        const emailError = validateEmail(email);
        const passwordError = validatePassword(password);
        setErrors({email: emailError, password: passwordError});
        return !emailError && !passwordError;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!validate()) return;

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

    return (
        <div className="flex flex-row justify-center">
            {ToasterElement}

            {/* Left side - Login Form */}
            <div className="flex flex-col flex-1 justify-center content-center gap-[8px] p-[12px]">

                <div className="flex flex-col items-center">
                    <Title3 block>Добро пожаловать</Title3>
                    <Text block>Войдите в свой аккаунт TimeCafe</Text>
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

                <Divider appearance="brand" className="divider">или продолжить с</Divider>

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
                        Нет аккаунта?{' '}
                        <Link
                            onClick={() => navigate("/register")}
                        >
                            Зарегистрироваться
                        </Link>
                    </Text>
                </div>

            </div>

            {/* Right side - Image (hidden on mobile) */}
            <div className="flex flex-col flex-1">
                <div className="">
                    <Title3 block className="text-white mb-6">TimeCafe</Title3>
                    <Text block size={500} className="mb-8 opacity-90">
                        Управляйте своим временем эффективно
                    </Text>
                    <div className="space-y-4">
                        <div className="flex items-center space-x-3">
                            <div
                                className="w-12 h-12 bg-white bg-opacity-20 rounded-lg flex items-center justify-center">
                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                          d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
                                </svg>
                            </div>
                            <Text size={400}>Почасовая аренда пространства</Text>
                        </div>
                        <div className="flex items-center space-x-3">
                            <div
                                className="w-12 h-12 bg-white bg-opacity-20 rounded-lg flex items-center justify-center">
                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                          d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
                                </svg>
                            </div>
                            <Text size={400}>Удобная система тарифов</Text>
                        </div>
                        <div className="flex items-center space-x-3">
                            <div
                                className="w-12 h-12 bg-white bg-opacity-20 rounded-lg flex items-center justify-center">
                                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                          d="M13 10V3L4 14h7v7l9-11h-7z"/>
                                </svg>
                            </div>
                            <Text size={400}>Быстрое бронирование</Text>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};
