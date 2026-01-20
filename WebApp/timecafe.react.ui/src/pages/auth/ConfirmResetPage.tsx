import {Button, Input, Field, Link, Body2, Caption1, Title3} from '@fluentui/react-components';
import {useState, useEffect, useCallback} from "react";
import {useNavigate, useLocation} from "react-router-dom";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {PasswordInput, ConfirmPasswordInput} from "../../components/FormFields";
import {authFormContainerClassName} from "../../layouts/authLayout";

export const ConfirmResetPage = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const {showToast, ToasterElement} = useProgressToast();

    const searchParams = new URLSearchParams(location.search);
    const email = searchParams.get("email") || "";
    const token = searchParams.get("token") || "";
    const [code, setCode] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [errors, setErrors] = useState({code: "", newPassword: "", confirmPassword: ""});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);

    useEffect(() => {
        if (!email || !token) {
            navigate("/login", {replace: true});
        }
    }, [email, token, navigate, location]);

    const validate = () => {
        const codeError = !code ? "Код обязателен" : "";
        setErrors(prev => ({...prev, code: codeError}));
        return !codeError && !errors.newPassword && !errors.confirmPassword;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(true);
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            // TODO: STUB - подтверждение кода и изменение пароля (подключить бек)
            // await confirmReset({email, code, newPassword});
            showToast("Пароль успешно изменен", "success");
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
                showToast("Ошибка. Попробуйте позже", "error");
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleNewPasswordValidationChange = useCallback((error: string) => {
        setErrors(prev => ({...prev, newPassword: error}));
    }, []);

    const handleConfirmPasswordValidationChange = useCallback((error: string) => {
        setErrors(prev => ({...prev, confirmPassword: error}));
    }, []);

    if (!email || !token) {
        return null;
    }

    return (
        <div
            className="!grid grid-cols-1 items-center justify-center
             sm:grid-cols-2 sm:justify-stretch sm:items-stretch">
            {ToasterElement}

            {/* Hero Section - Left Side (Desktop Only) */}
            <div id="Left Side" className="relative hidden sm:block bg-[url(/src/assets/abstract_bg.svg)] bg-left bg-cover bg-no-repeat">
                <div className="absolute inset-0 bg-black/40 pointer-events-none" />
            </div>

            {/* Confirm Reset Form */}
            <div id="Form"
                  className={authFormContainerClassName}>
                <div className="flex flex-col w-full max-w-md gap-[12px]">

                    <div className="flex flex-col items-center">
                        <Title3 block>Установить новый пароль</Title3>
                        <Body2 block>Восстановление доступа для {email}</Body2>
                    </div>

                    <form onSubmit={handleSubmit}>
                        <Field
                            label="Код из письма"
                            required
                            validationState={errors.code ? "error" : undefined}
                            validationMessage={errors.code}
                        >
                            <Input
                                type="text"
                                value={code}
                                onChange={(_, data) => setCode(data.value)}
                                placeholder="Введите код"
                                disabled={isSubmitting}
                                className="w-full"
                            />
                        </Field>

                        <PasswordInput
                            value={newPassword}
                            onChange={setNewPassword}
                            disabled={isSubmitting}
                            label="Новый пароль"
                            placeholder="Введите новый пароль"
                            onValidationChange={handleNewPasswordValidationChange}
                            shouldValidate={submitted}
                        />

                        <ConfirmPasswordInput
                            value={confirmPassword}
                            onChange={setConfirmPassword}
                            passwordValue={newPassword}
                            disabled={isSubmitting}
                            onValidationChange={handleConfirmPasswordValidationChange}
                            shouldValidate={submitted}
                        />

                        <Button
                            appearance="primary"
                            type="submit"
                            disabled={isSubmitting}
                            className="w-full mt-4"
                        >
                            {isSubmitting ? "Восстановление..." : "Восстановить пароль"}
                        </Button>
                    </form>

                    <div>
                        <Caption1>
                            <Link
                                onClick={() => navigate("/login")}
                            >
                                Вернуться к входу
                            </Link>
                        </Caption1>
                    </div>
                </div>
            </div>
        </div>
    );
};
