import {Link, Body2, Caption1, Title3, Divider, Spinner} from '@fluentui/react-components';
import {useState, useCallback} from "react";
import {useNavigate} from "react-router-dom";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {EmailInput, PasswordInput} from "../../components/FormFields";
import {useDispatch} from "react-redux";
import {authFormContainerClassName} from "../../layouts/authLayout";
import {authApi} from "../../shared/api/auth/authApi";
import {getUserMessageFromUnknown} from "../../shared/api/errors/getUserMessageFromUnknown";
import {setAccessToken, setEmail, setEmailConfirmed, setRole, setUserId} from "../../store/authSlice";
import {getJwtInfo} from "../../shared/auth/jwt";
import {getApiBaseUrl} from "../../shared/api/apiBaseUrl";
import {TooltipButton} from "../../components/TooltipButton/TooltipButton";

export const LoginPage = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const dispatch = useDispatch();

    const [email, setEmailValue] = useState("");
    const [password, setPassword] = useState("");
    const [errors, setErrors] = useState({email: "", password: ""});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);

    const apiBase = getApiBaseUrl();
    const returnUrl = `${window.location.origin}/external-callback`;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(true);
        if (errors.email || errors.password || !email || !password) return;

        setIsSubmitting(true);
        try {
            const r = await authApi.loginJwtV2({email, password});
            if (r.emailConfirmed === false) {
                dispatch(setEmail(email));
                dispatch(setEmailConfirmed(false));
                navigate("/email-pending", {replace: true});
                return;
            }

            dispatch(setAccessToken(r.accessToken));
            const info = getJwtInfo(r.accessToken);
            if (info.userId) dispatch(setUserId(info.userId));
            if (info.role) dispatch(setRole(info.role));
            dispatch(setEmail(info.email ?? email));
            dispatch(setEmailConfirmed(typeof r.emailConfirmed === "boolean" ? r.emailConfirmed : false));
            navigate("/home");
        } catch (err: unknown) {
            showToast(getUserMessageFromUnknown(err), "error", "Ошибка");
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleGoogleLogin = () => {
        window.location.href = `${apiBase}/auth/authenticate/login/google?returnUrl=${encodeURIComponent(returnUrl)}`;
    };

    const handleMicrosoftLogin = () => {
        window.location.href = `${apiBase}/auth/authenticate/login/microsoft?returnUrl=${encodeURIComponent(returnUrl)}`;
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
                        onChange={setEmailValue}
                        disabled={isSubmitting}
                        onValidationChange={handleEmailValidationChange}
                        shouldValidate={submitted}
                    />

                    <PasswordInput
                        value={password}
                        onChange={setPassword}
                        disabled={isSubmitting}
                        onValidationChange={handlePasswordValidationChange}
                        shouldValidate={false}
                    />

                    <div className="flex items-center justify-between">
                        <Link
                            onClick={() => navigate("/reset-password")}
                            className=""
                        >
                            Забыли пароль?
                        </Link>
                    </div>

                    <TooltipButton
                        appearance="primary"
                        type="submit"
                        disabled={isSubmitting}
                        className="sm:w-full"
                        icon={isSubmitting ? <Spinner size="tiny" /> : undefined}
                        tooltip="Войти"
                        label="Войти"
                    />

                    <Divider appearance="brand" className="divider grow-0">или продолжить с</Divider>

                    <div>
                        <div className="grid grid-cols-1 gap-[12px] sm:grid-cols-2">
                            <TooltipButton
                                appearance="secondary"
                                onClick={handleGoogleLogin}
                                disabled={isSubmitting}
                                icon={<i className="icons8-google"/>}
                                tooltip="Войти через Google"
                                label="Google"
                            />

                            <TooltipButton
                                appearance="secondary"
                                onClick={handleMicrosoftLogin}
                                disabled={isSubmitting}
                                icon={<i className="icons8-microsoft"/>}
                                tooltip="Войти через Microsoft"
                                label="Microsoft"
                            />
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
            <div id="Right Side" className="relative hidden sm:block bg-[url(/src/assets/abstract_bg.svg)] bg-right bg-cover bg-no-repeat">
                <div className="absolute inset-0 bg-black/40 pointer-events-none" />
            </div>
        </div>
    );
};
