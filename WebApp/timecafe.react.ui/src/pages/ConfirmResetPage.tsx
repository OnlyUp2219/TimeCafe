import {Button, Input, Field, Link, Text, Title3} from '@fluentui/react-components';
import {useState, useEffect} from "react";
import {useNavigate, useLocation} from "react-router-dom";
import {validatePassword} from "../utility/validate.ts";
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";

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

    useEffect(() => {
        if (!email || !token) {
            const from = location.state?.from || "/login";
            navigate(from, {replace: true});
        }
    }, [email, token, navigate, location]);

    const validate = () => {
        const codeError = !code ? "Код обязателен" : "";
        const passwordError = validatePassword(newPassword);
        const confirmPasswordError = newPassword !== confirmPassword ? "Пароли не совпадают" : "";
        setErrors({code: codeError, newPassword: passwordError, confirmPassword: confirmPasswordError});
        return !codeError && !passwordError && !confirmPasswordError;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
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

    if (!email || !token) {
        return null;
    }

    return (
        <div
            className="!grid grid-cols-1 items-center justify-center
             sm:grid-cols-2 sm:justify-stretch sm:items-stretch">
            {ToasterElement}

            {/* Hero Section - Left Side (Desktop Only) */}
            <div id="Left Side" className="hidden sm:block bg-sky-400">
            </div>

            {/* Confirm Reset Form */}
            <div id="Form"
                 className="flex flex-col flex-wrap items-center w-full
                 sm:w-auto sm:justify-center sm:p-8">
                <div className="flex flex-col w-full max-w-md gap-[12px]">

                    <div className="flex flex-col items-center">
                        <Title3 block>Установить новый пароль</Title3>
                        <Text block>Восстановление доступа для {email}</Text>
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

                        <Field
                            label="Новый пароль"
                            required
                            validationState={errors.newPassword ? "error" : undefined}
                            validationMessage={errors.newPassword}
                        >
                            <Input
                                type="password"
                                value={newPassword}
                                onChange={(_, data) => setNewPassword(data.value)}
                                placeholder="Введите новый пароль"
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
                            type="submit"
                            disabled={isSubmitting}
                            className="w-full mt-4"
                        >
                            {isSubmitting ? "Восстановление..." : "Восстановить пароль"}
                        </Button>
                    </form>

                    <div>
                        <Text size={300}>
                            <Link
                                onClick={() => navigate("/login")}
                            >
                                Вернуться к входу
                            </Link>
                        </Text>
                    </div>
                </div>
            </div>
        </div>
    );
};
