import {Button, Link, Body2, Caption1, Title3, Divider} from '@fluentui/react-components';
import {useState, useCallback} from "react";
import {useNavigate} from "react-router-dom";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {EmailInput, PasswordInput, ConfirmPasswordInput} from "../../components/FormFields";
import {authFormContainerClassName} from "../../layouts/authLayout";
import {authApi} from "../../shared/api/auth/authApi";
import {getUserMessageFromUnknown} from "../../shared/api/errors/getUserMessageFromUnknown";
import {useDispatch} from "react-redux";
import {setEmail, setEmailConfirmed} from "../../store/authSlice";
import {normalizeUnknownError} from "../../shared/api/errors/normalize";
import {isApiError} from "../../shared/api/errors/types";

export const RegisterPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const dispatch = useDispatch();

    const apiBase = import.meta.env.VITE_API_BASE_URL;
    const returnUrl = `${window.location.origin}/external-callback`;

    const [email, setEmailValue] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [errors, setErrors] = useState({email: "", password: "", confirmPassword: ""});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);
    const [serverErrors, setServerErrors] = useState<{ email?: string; password?: string; confirmPassword?: string; general?: string }>({});


    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(true);
        setServerErrors({});
        if (errors.email || errors.password || errors.confirmPassword || !email || !password) return;

        setIsSubmitting(true);
        try {
            const r = await authApi.registerWithUsername({username: email, email, password});
            dispatch(setEmail(email));
            dispatch(setEmailConfirmed(false));
            navigate("/email-pending", {replace: true, state: {mockLink: r.callbackUrl}});
        } catch (err: unknown) {
            const apiErr = isApiError(err) ? err : normalizeUnknownError(err);
            const fieldErrors: { email?: string; password?: string; confirmPassword?: string; general?: string } = {};

            const appendError = (key: "email" | "password" | "confirmPassword" | "general", msg: string) => {
                if (!msg) return;
                const current = fieldErrors[key];
                if (!current) {
                    fieldErrors[key] = msg;
                    return;
                }
                if (current.includes(msg)) return;
                fieldErrors[key] = `${current} ${msg}`;
            };

            for (const item of apiErr.errors ?? []) {
                const code = (item.code || "").toLowerCase();
                const msg = item.message;

                if (code.includes("duplicateemail") || code.includes("duplicateusername")) {
                    appendError("email", msg);
                    continue;
                }

                if (code.startsWith("password") || code.includes("password")) {
                    appendError("password", msg);
                    continue;
                }
            }

            if (Object.keys(fieldErrors).length) {
                setServerErrors(fieldErrors);
            } else {
                showToast(getUserMessageFromUnknown(err), "error", "Ошибка");
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

    const handleConfirmPasswordValidationChange = useCallback((error: string) => {
        setErrors(prev => ({...prev, confirmPassword: error}));
    }, []);

    return (
        <div
            className="!grid grid-cols-1 items-center justify-center
             sm:grid-cols-2 sm:justify-stretch sm:items-stretch ">
            {ToasterElement}

            {/* Hero Section - Left Side (Desktop Only) */}
            <div id="Left Side" className="relative hidden sm:block bg-[url(/src/assets/abstract_bg.svg)] bg-left bg-cover bg-no-repeat">
                <div className="absolute inset-0 bg-black/40 pointer-events-none" />
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
                            onChange={setEmailValue}
                        disabled={isSubmitting}
                        onValidationChange={handleEmailValidationChange}
                        shouldValidate={submitted}
                        externalError={serverErrors.email}
                    />

                    <PasswordInput
                        value={password}
                        onChange={setPassword}
                        disabled={isSubmitting}
                        showRequirements={true}
                        onValidationChange={handlePasswordValidationChange}
                        shouldValidate={submitted}
                        externalError={serverErrors.password}
                    />

                    <ConfirmPasswordInput
                        value={confirmPassword}
                        onChange={setConfirmPassword}
                        passwordValue={password}
                        disabled={isSubmitting}
                        onValidationChange={handleConfirmPasswordValidationChange}
                        shouldValidate={submitted}
                        externalError={serverErrors.confirmPassword}
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