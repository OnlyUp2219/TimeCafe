import {useNavigate} from "react-router-dom";
import {Button, Card, Field, Input, Subtitle1, Text} from "@fluentui/react-components";
import {validateEmail} from "../../utility/validate.ts";
import {forgotPassword} from "../../api/auth.ts";
import {useState} from "react";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {parseErrorMessage} from "../../utility/errors.ts";
import {useRateLimitedRequest} from "../../hooks/useRateLimitedRequest.ts";

export const ResetPasswordEmail = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();

    const [email, setEmail] = useState("klimenkokov1@timecafesharp.ru");
    const [errors, setErrors] = useState({
        email: "",
    });
    const [isSent, setIsSent] = useState(false);
    const useMockEmail = import.meta.env.VITE_USE_MOCK_EMAIL === "true";

    const {data, countdown, isBlocked, isLoading, sendRequest} = useRateLimitedRequest(
        'forgot-password',
        async () => {
            const response = await forgotPassword({email});
            return response;
        }
    );

    const validate = () => {
        const emailError = validateEmail(email);
        setErrors({email: emailError});
        return !emailError;
    };

    const handleSubmit = async () => {
        if (!validate()) return;

        if (isBlocked) {
            showToast(`Подождите ${countdown} сек. перед повторной отправкой`, "warning");
            return;
        }

        try {
            await sendRequest();
            setIsSent(true);
        } catch (err: any) {
            if (err.status === 429) {
                showToast(err.message || `Подождите ${err.retryAfter || countdown} сек.`, "error");
                return;
            }

            const newErrors = {email: ""};
            if (Array.isArray(err)) {
                err.forEach((e: { code: string; description: string }) => {
                    const code = e.code.toLowerCase();
                    if (code.includes("email")) newErrors.email += e.description + " ";
                });
            } else {
                const message = parseErrorMessage(err);
                showToast(message, "error");
            }
            setErrors(newErrors);
        }
    };

    if (isSent) {
        return (
            <Card className="auth_card p-6">
                {ToasterElement}

                <Subtitle1 align="center" style={{marginBottom: 16}}>Сообщение отправлено!</Subtitle1>

                <Text>
                    Письмо для сброса пароля отправлено на <strong>{email}</strong>.
                </Text>

                {useMockEmail && data?.callbackUrl && (
                    <Text className="flex flex-wrap break-words">
                        <a className="break-all hyphens-auto" href={data.callbackUrl}>Ссылка</a> для сброса пароля (Mock
                        режим).
                    </Text>
                )}

                <Text>
                    Проверьте свой почтовый ящик, включая папку "Спам". Ссылка для восстановления пароля действительна
                    ограниченное время.
                </Text>

                <div className="button-action">
                    <Button
                        className="flex-[1]"
                        appearance="secondary"
                        onClick={() => window.open("https://mail.google.com", "_blank")}
                        type="button"
                    >
                        Перейти в Gmail
                    </Button>

                    <Button
                        className="flex-[1.5]"
                        appearance="primary"
                        onClick={() => navigate("/login")}
                        type="button"
                    >
                        Продолжить
                    </Button>
                </div>
            </Card>
        );
    }

    return (
        <Card className="auth_card">
            {ToasterElement}

            <Subtitle1 align={"center"}>Забыли пароль?</Subtitle1>

            <Field label="Почта"
                   required
                   validationState={errors.email ? "error" : undefined}
                   validationMessage={errors.email}
                   hint={countdown > 0
                       ? `Повторная отправка через ${countdown} сек.`
                       : ""}
            >
                <Input
                    value={email}
                    placeholder="Введите почту"
                    type="email"
                    onChange={(_, data) => setEmail(data.value)}
                    disabled={isBlocked}
                />
            </Field>

            <div className="button-action">
                <Button className="flex-[1] "
                        appearance="secondary"
                        onClick={() => navigate(-1)}
                        type="button">Назад</Button>

                <Button
                    className="flex-[1.5]"
                    appearance="primary"
                    onClick={handleSubmit}
                    disabled={isLoading || isBlocked}
                    type="button">
                    {countdown > 0
                        ? `К сообщению`
                        : "Продолжить"}
                </Button>
            </div>

        </Card>
    );
};