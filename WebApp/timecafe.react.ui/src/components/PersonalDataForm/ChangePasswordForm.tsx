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
import {useErrorHandler} from "../../hooks/useErrorHandler.ts";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";

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
    const {showToast} = useProgressToast();
    const {fieldErrors, handleError, clearAllErrors} = useErrorHandler(showToast);

    const [currentPassword, setCurrentPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [success, setSuccess] = useState(false);
    const [loading, setLoading] = useState(false);

    const [localErrors, setLocalErrors] = useState({
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",
    });

    const dispatch = useDispatch<AppDispatch>();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSuccess(false);

        setLocalErrors({
            currentPassword: "",
            newPassword: "",
            confirmPassword: "",
        });

        if (!currentPassword.trim()) {
            setLocalErrors(prev => ({...prev, currentPassword: "Введите текущий пароль."}));
            return;
        }

        const newPasswordValidation = validatePassword(newPassword);
        if (newPasswordValidation) {
            setLocalErrors(prev => ({...prev, newPassword: newPasswordValidation}));
            return;
        }

        const confirmPasswordValidation = validateConfirmPassword(confirmPassword, newPassword);
        if (confirmPasswordValidation) {
            setLocalErrors(prev => ({...prev, confirmPassword: confirmPasswordValidation}));
            return;
        }

        clearAllErrors();
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
        } catch (error) {
            handleError(error);
        } finally {
            setLoading(false);
        }
    };

    const allErrors = {...localErrors, ...fieldErrors};

    return (
        <Card className={className}>
            <Title2>Смена пароля</Title2>
            {success && (
                <MessageBar intent="success">
                    <MessageBarBody>
                        <MessageBarTitle>Успешно</MessageBarTitle>
                        Пароль изменён. {redirectToLoginOnSuccess ? "Перенаправление..." : ""}
                    </MessageBarBody>
                </MessageBar>
            )}
            <form onSubmit={handleSubmit} className="flex flex-col gap-[16px] mt-[8px] ">
                <Field
                    label="Текущий пароль"
                    required
                    validationMessage={allErrors.currentPassword || undefined}
                    validationState={allErrors.currentPassword ? "error" : "none"}
                >
                    <Input
                        type="password"
                        value={currentPassword}
                        onChange={(e) => setCurrentPassword(e.target.value)}
                        placeholder="Введите текущий пароль"
                        disabled={loading || success}
                    />
                </Field>
                <Field
                    label="Новый пароль"
                    required
                    validationMessage={allErrors.newPassword || undefined}
                    validationState={allErrors.newPassword ? "error" : "none"}
                >
                    <Input
                        type="password"
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        placeholder="Введите новый пароль"
                        disabled={loading || success}
                    />
                </Field>
                <Field
                    label="Подтвердите новый пароль"
                    required
                    validationMessage={allErrors.confirmPassword || undefined}
                    validationState={allErrors.confirmPassword ? "error" : "none"}
                >
                    <Input
                        type="password"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        placeholder="Введите новый пароль ещё раз"
                        disabled={loading || success}
                    />
                </Field>
                <div className="button-action justify-stretch!important ">
                    <Button appearance="primary" type="submit" disabled={loading || success}>
                        {loading ? "Сохранение..." : "Сменить пароль"}
                    </Button>
                    {showCancelButton && (
                        <Button appearance="secondary" type="button" onClick={onCancel}
                                disabled={loading}>
                            Отмена
                        </Button>
                    )}
                </div>
            </form>
        </Card>
    );
};
