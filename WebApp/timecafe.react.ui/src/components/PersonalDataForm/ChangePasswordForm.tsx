import {useState} from "react";
import type {FC} from "react";
import {
    Button,
    Field,
    Input,
    Card,
    MessageBar,
    MessageBarBody,
    MessageBarTitle,
    Title2
} from "@fluentui/react-components";
import {changePassword} from "../../api/auth.ts";
import {useDispatch} from "react-redux";
import type {AppDispatch} from "../../store";
import {useNavigate} from "react-router-dom";
import {clearTokens} from "../../store/authSlice.ts";
import {validatePassword, validateConfirmPassword} from "../../utility/validate.ts";
import {parseErrorMessage} from "../../utility/errors.ts";

export interface ChangePasswordFormProps {
    redirectToLoginOnSuccess?: boolean;
    autoClearTokensOnSuccess?: boolean;
    onSuccess?: () => void;
    onCancel?: () => void;
    showCancelButton?: boolean;
    className?: string;
}

export const ChangePasswordForm: FC<ChangePasswordFormProps> = ({
                                                                    redirectToLoginOnSuccess = true,
                                                                    autoClearTokensOnSuccess = true,
                                                                    onSuccess,
                                                                    onCancel,
                                                                    showCancelButton = false,
                                                                    className,
                                                                }) => {
    const [currentPassword, setCurrentPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);
    const [loading, setLoading] = useState(false);
    
    const [currentPasswordError, setCurrentPasswordError] = useState("");
    const [newPasswordError, setNewPasswordError] = useState("");
    const [confirmPasswordError, setConfirmPasswordError] = useState("");

    const dispatch = useDispatch<AppDispatch>();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setSuccess(false);
        setCurrentPasswordError("");
        setNewPasswordError("");
        setConfirmPasswordError("");

        if (!currentPassword.trim()) {
            setCurrentPasswordError("Введите текущий пароль.");
            return;
        }

        const newPasswordValidation = validatePassword(newPassword);
        if (newPasswordValidation) {
            setNewPasswordError(newPasswordValidation);
            return;
        }

        const confirmPasswordValidation = validateConfirmPassword(confirmPassword, newPassword);
        if (confirmPasswordValidation) {
            setConfirmPasswordError(confirmPasswordValidation);
            return;
        }

        setLoading(true);
        try {
            await changePassword({currentPassword, newPassword});
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
            setError(parseErrorMessage(err) || "Ошибка при смене пароля. Проверьте текущий пароль.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className={className}>
            <Title2>Смена пароля</Title2>
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
            <form onSubmit={handleSubmit} className="flex flex-col gap-[16px] mt-[8px]">
                <Field 
                    label="Текущий пароль" 
                    required
                    validationMessage={currentPasswordError || undefined}
                    validationState={currentPasswordError ? "error" : "none"}
                >
                    <Input
                        type="password"
                        value={currentPassword}
                        onChange={(e) => {
                            setCurrentPassword(e.target.value);
                            setCurrentPasswordError("");
                        }}
                        placeholder="Введите текущий пароль"
                        disabled={loading || success}
                    />
                </Field>
                <Field 
                    label="Новый пароль" 
                    required
                    validationMessage={newPasswordError || undefined}
                    validationState={newPasswordError ? "error" : "none"}
                >
                    <Input
                        type="password"
                        value={newPassword}
                        onChange={(e) => {
                            setNewPassword(e.target.value);
                            setNewPasswordError("");
                        }}
                        placeholder="Введите новый пароль"
                        disabled={loading || success}
                    />
                </Field>
                <Field 
                    label="Подтвердите новый пароль" 
                    required
                    validationMessage={confirmPasswordError || undefined}
                    validationState={confirmPasswordError ? "error" : "none"}
                >
                    <Input
                        type="password"
                        value={confirmPassword}
                        onChange={(e) => {
                            setConfirmPassword(e.target.value);
                            setConfirmPasswordError("");
                        }}
                        placeholder="Введите новый пароль ещё раз"
                        disabled={loading || success}
                    />
                </Field>
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
        </Card>
    );
};
