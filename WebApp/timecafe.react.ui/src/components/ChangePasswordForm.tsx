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
import {changePassword} from "../api/auth";
import {useDispatch} from "react-redux";
import type {AppDispatch} from "../store";
import {useNavigate} from "react-router-dom";
import {clearTokens} from "../store/authSlice";

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

    const dispatch = useDispatch<AppDispatch>();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setSuccess(false);

        if (!currentPassword || !newPassword || !confirmPassword) {
            setError("Заполните все поля");
            return;
        }
        if (newPassword !== confirmPassword) {
            setError("Новые пароли не совпадают");
            return;
        }
        if (newPassword.length < 6) {
            setError("Новый пароль должен быть не менее 6 символов");
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
        } catch (err: any) {
            if (err?.errors && Array.isArray(err.errors)) {
                const msgs = err.errors
                    .map((e: { code: string; description: string }) => e.description)
                    .join(", ");
                setError(msgs);
            } else if (typeof err === "string") {
                setError(err);
            } else if (err?.message) {
                setError(err.message);
            } else {
                setError("Ошибка при смене пароля. Проверьте текущий пароль.");
            }
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
                <Field label="Текущий пароль" required>
                    <Input
                        type="password"
                        value={currentPassword}
                        onChange={(e) => setCurrentPassword(e.target.value)}
                        placeholder="Введите текущий пароль"
                        disabled={loading || success}
                    />
                </Field>
                <Field label="Новый пароль" required>
                    <Input
                        type="password"
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        placeholder="Введите новый пароль"
                        disabled={loading || success}
                    />
                </Field>
                <Field label="Подтвердите новый пароль" required>
                    <Input
                        type="password"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
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
