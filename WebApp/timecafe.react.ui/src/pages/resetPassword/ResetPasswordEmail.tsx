import {useNavigate} from "react-router-dom";
import {Button, Card, Field, Input, Subtitle1, Text} from "@fluentui/react-components";
import {validateEmail} from "../../utility/validate.ts";
import {forgotPassword} from "../../api/auth.ts";
import {useEffect, useState} from "react";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress.tsx";
import {RateLimiter} from "../../utility/rateLimitHelpers.ts";
import {useErrorHandler} from "../../hooks/useErrorHandler.ts";

const emailRateLimiter = new RateLimiter("email_rate_limit");
const RATE_LIMIT_SECONDS = parseInt(import.meta.env.VITE_EMAIL_RATE_LIMIT_SECONDS || "60");

export const ResetPasswordEmail = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();
    const {fieldErrors, handleError, clearAllErrors} = useErrorHandler(showToast);

    const [email, setEmail] = useState("");
    const [localErrors, setLocalErrors] = useState({
        email: "",
    });
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isSent, setIsSent] = useState(false);
    const [link, setLink] = useState<string | null>(null);
    const [remainingTime, setRemainingTime] = useState(0);
    const useMockEmail = import.meta.env.VITE_USE_MOCK_EMAIL === "true";

    // Todo убрать после тестов
    useEffect(() => {
        setEmail("klimenkokov1@timecafesharp.ru");
    }, []);

    useEffect(() => {
        if (email) {
            const saved = emailRateLimiter.getSavedRateLimit();

            if (saved.identifier === email) {
                const remaining = emailRateLimiter.getRemainingTime(RATE_LIMIT_SECONDS);
                setRemainingTime(remaining);

                if (remaining > 0) {
                    const interval = setInterval(() => {
                        const newRemaining = emailRateLimiter.getRemainingTime(RATE_LIMIT_SECONDS);
                        setRemainingTime(newRemaining);
                        if (newRemaining === 0) {
                            clearInterval(interval);
                        }
                    }, 1000);
                    return () => clearInterval(interval);
                }
            } else {
                setRemainingTime(0);
            }
        }
    }, [email]);
    const validate = () => {
        const emailError = validateEmail(email);
        setLocalErrors({email: emailError});
        return !emailError;
    };

    const handleSubmit = async () => {
        if (!validate()) return;

        if (remainingTime > 0) {
            setIsSent(true);
            return;
        }

        if (!emailRateLimiter.canProceed(email, RATE_LIMIT_SECONDS)) {
            const remaining = emailRateLimiter.getRemainingTime(RATE_LIMIT_SECONDS);
            showToast(`Подождите ${remaining} сек. перед повторной отправкой`, "warning");
            return;
        }

        clearAllErrors();
        setIsSubmitting(true);
        try {
            const res = await forgotPassword({email});

            emailRateLimiter.saveRateLimit(email, Date.now());
            setRemainingTime(RATE_LIMIT_SECONDS);

            setIsSent(true);
            setLink(res.callbackUrl ?? null);
        } catch (error) {
            handleError(error);
        } finally {
            setIsSubmitting(false);
        }
    };

    const allErrors = {...localErrors, ...fieldErrors};

    if (isSent) {
        return (
            <Card className="auth_card p-6">
                {ToasterElement}

                <Subtitle1 align="center" style={{marginBottom: 16}}>Сообщение отправлено!</Subtitle1>

                <Text>
                    Письмо для сброса пароля отправлено на <strong>{email}</strong>.
                </Text>

                {useMockEmail && link && (
                    <Text className="flex flex-wrap break-words">
                        <a className="break-all hyphens-auto" href={link}>Ссылка</a> для сброса пароля (Mock режим).
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


    if (!isSent) {
        return (
            <Card className="auth_card">
                {ToasterElement}

                <Subtitle1 align={"center"}>Забыли пароль?</Subtitle1>

                <Field label="Почта"
                       required
                       validationState={allErrors.email ? "error" : undefined}
                       validationMessage={allErrors.email}
                       hint={remainingTime > 0
                           ? `Повторная отправка через ${remainingTime} сек.`
                           : ""}
                >
                    <Input
                        value={email}
                        placeholder="Введите почту"
                        type="email"
                        onChange={(_, data) => setEmail(data.value)}
                        disabled={remainingTime > 0}
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
                        disabled={isSubmitting}
                        type="button"
                    >
                        {remainingTime > 0
                            ? `К сообщению`
                            : "Продолжить"}
                    </Button>
                </div>

            </Card>
        )
    }
}