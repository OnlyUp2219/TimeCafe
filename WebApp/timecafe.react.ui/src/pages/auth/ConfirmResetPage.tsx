import {Body2, Title3, Spinner} from '@fluentui/react-components';
import React, {useState, useEffect, useCallback} from "react";
import {useNavigate, useLocation} from "react-router-dom";
import {useProgressToast} from "@components/ToastProgress/ToastProgress.tsx";
import {PasswordInput, ConfirmPasswordInput} from "@components/FormFields";
import {authFormContainerClassName} from "@layouts/authLayout";
import {authApi} from "@api/auth/authApi";
import {TooltipButton} from "@components/TooltipButton/TooltipButton";
import {getUserMessageFromUnknown} from "@api/errors/getUserMessageFromUnknown";

export const ConfirmResetPage = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const {showToast, ToasterElement} = useProgressToast();

    const searchParams = new URLSearchParams(location.search);
    const email = searchParams.get("email") || "";
    const resetCode = searchParams.get("code") || "";
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [errors, setErrors] = useState({newPassword: "", confirmPassword: ""});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitted, setSubmitted] = useState(false);

    useEffect(() => {
        if (!email || !resetCode) {
            navigate("/login", {replace: true});
        }
    }, [email, resetCode, navigate, location]);

    const validate = () => {
        return !errors.newPassword && !errors.confirmPassword;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(true);
        if (!validate()) return;

        setIsSubmitting(true);
        try {
            await authApi.resetPassword({email, resetCode, newPassword});
            showToast("Пароль успешно изменен", "success");
            navigate("/login");
        } catch (err: unknown) {
            showToast(getUserMessageFromUnknown(err), "error");
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

    if (!email || !resetCode) {
        return null;
    }

    return (
        <div
            className="!grid grid-cols-1 items-center justify-center
             sm:grid-cols-2 sm:justify-stretch sm:items-stretch">
            {ToasterElement}

            {/* Hero Section - Left Side (Desktop Only) */}
            <div id="Left Side"
                 className="relative hidden sm:block bg-[url(/src/assets/abstract_bg.svg)] bg-left bg-cover bg-no-repeat">
                <div className="absolute inset-0 bg-black/40 pointer-events-none"/>
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

                        <div className="grid grid-cols-1 gap-[12px] mt-4 sm:grid-cols-2">
                            <TooltipButton
                                appearance="primary"
                                type="submit"
                                disabled={isSubmitting}
                                className="w-full order-1 sm:order-2"
                                icon={isSubmitting ? <Spinner size="tiny"/> : undefined}
                                tooltip="Сохранить новый пароль"
                                label="Восстановить пароль"
                            />

                            <TooltipButton
                                appearance="secondary"
                                type="button"
                                disabled={isSubmitting}
                                className="w-full order-2 sm:order-1"
                                onClick={() => navigate("/login")}
                                tooltip="Вернуться на страницу входа"
                                label="Вернуться к входу"
                            />
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};
