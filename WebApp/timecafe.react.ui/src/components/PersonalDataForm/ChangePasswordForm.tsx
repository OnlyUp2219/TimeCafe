import React, {useCallback, useState} from "react";
import type {FC} from "react";
import {
    Badge,
    Button,
    Card,
    MessageBar,
    MessageBarBody,
    MessageBarTitle,
    Title2
} from "@fluentui/react-components";
import {LockClosedRegular} from "@fluentui/react-icons";
import {useDispatch} from "react-redux";
import type {AppDispatch} from "../../store";
import {useNavigate} from "react-router-dom";
import {clearTokens} from "../../store/authSlice.ts";
import {ConfirmPasswordInput, PasswordInput} from "../FormFields";
import {authApi} from "../../shared/api/auth/authApi";
import {getUserMessageFromUnknown} from "../../shared/api/errors/getUserMessageFromUnknown";

export interface ChangePasswordFormProps {
    redirectToLoginOnSuccess?: boolean;
    autoClearTokensOnSuccess?: boolean;
    onSuccess?: () => void;
    onCancel?: () => void;
    showCancelButton?: boolean;
    className?: string;
    wrapInCard?: boolean;
    showTitle?: boolean;
}

export const ChangePasswordForm: FC<ChangePasswordFormProps> = ({
                                                                    redirectToLoginOnSuccess = true,
                                                                    autoClearTokensOnSuccess = true,
                                                                    onSuccess,
                                                                    onCancel,
                                                                    showCancelButton = false,
                                                                    className,
                                                                    wrapInCard = true,
                                                                    showTitle = true,
                                                                }) => {
    const [currentPassword, setCurrentPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);
    const [loading, setLoading] = useState(false);

    const [submitted, setSubmitted] = useState(false);
    const [errors, setErrors] = useState({currentPassword: "", newPassword: "", confirmPassword: ""});

    const dispatch = useDispatch<AppDispatch>();
    const navigate = useNavigate();

    const validateCurrentPassword = useCallback((pwd: string) => (pwd.trim() ? "" : "Введите текущий пароль."), []);
    const handleCurrentPasswordValidation = useCallback((err: string) => {
        setErrors((prev) => ({...prev, currentPassword: err}));
    }, []);
    const handleNewPasswordValidation = useCallback((err: string) => {
        setErrors((prev) => ({...prev, newPassword: err}));
    }, []);
    const handleConfirmPasswordValidation = useCallback((err: string) => {
        setErrors((prev) => ({...prev, confirmPassword: err}));
    }, []);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitted(true);
        setError(null);
        setSuccess(false);

        if (
            !currentPassword.trim() ||
            !newPassword.trim() ||
            !confirmPassword.trim() ||
            errors.currentPassword ||
            errors.newPassword ||
            errors.confirmPassword
        ) {
            return;
        }

        setLoading(true);

        try {
            await authApi.changePassword({currentPassword, newPassword});

            setSuccess(true);
            onSuccess?.();

            if (autoClearTokensOnSuccess) {
                dispatch(clearTokens());
            }

            if (redirectToLoginOnSuccess) {
                setTimeout(() => {
                    navigate("/login");
                }, 1500);
            }
        } catch (err: unknown) {
            setError(getUserMessageFromUnknown(err) || "Ошибка при смене пароля. Проверьте текущий пароль.");
        } finally {
            setLoading(false);
        }
    };

    const body = (
        <>
            {showTitle && (
                <Title2 block className="!flex items-center gap-2">
                    <Badge appearance="tint" shape="rounded" size="extra-large" className="brand-badge">
                        <LockClosedRegular className="size-5"/>
                    </Badge>
                    Смена пароля
                </Title2>
            )}
            {error && (
                <MessageBar intent="error">
                    <MessageBarBody>
                        <MessageBarTitle>Ошибка</MessageBarTitle>
                        {error}
                    </MessageBarBody>
                </MessageBar>
            )}
            {success && (
                <MessageBar intent="success">
                    <MessageBarBody>
                        <MessageBarTitle>Успешно</MessageBarTitle>
                        Пароль изменён. {redirectToLoginOnSuccess ? "Перенаправление..." : ""}
                    </MessageBarBody>
                </MessageBar>
            )}

            <form onSubmit={handleSubmit}
                  className={showTitle ? "flex flex-col gap-[12px] mt-[8px]" : "flex flex-col gap-[12px]"}>
                <PasswordInput
                    value={currentPassword}
                    onChange={setCurrentPassword}
                    disabled={loading || success}
                    label="Текущий пароль"
                    placeholder="Введите текущий пароль"
                    shouldValidate={submitted}
                    validate={validateCurrentPassword}
                    onValidationChange={handleCurrentPasswordValidation}
                />

                <PasswordInput
                    value={newPassword}
                    onChange={setNewPassword}
                    disabled={loading || success}
                    label="Новый пароль"
                    placeholder="Введите новый пароль"
                    shouldValidate={submitted}
                    showRequirements
                    onValidationChange={handleNewPasswordValidation}
                />

                <ConfirmPasswordInput
                    value={confirmPassword}
                    onChange={setConfirmPassword}
                    passwordValue={newPassword}
                    disabled={loading || success}
                    label="Подтвердите новый пароль"
                    placeholder="Введите новый пароль ещё раз"
                    shouldValidate={submitted}
                    onValidationChange={handleConfirmPasswordValidation}
                />

                <div className="flex gap-[12px]">
                    <Button appearance="primary" type="submit" disabled={loading || success}>
                        {loading ? "Сохранение..." : "Сменить пароль"}
                    </Button>
                    {showCancelButton && (
                        <Button appearance="secondary" type="button" onClick={onCancel} disabled={loading}>
                            Отмена
                        </Button>
                    )}
                </div>
            </form>
        </>
    );

    if (!wrapInCard) {
        return <div className={className}>{body}</div>;
    }

    return (
        <Card className={className}>
            {body}
        </Card>
    );
};
