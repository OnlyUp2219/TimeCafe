import {useEffect, useMemo, useState, type FC} from "react";
import {
    Body1,
    Button,
    Caption1,
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
    Field,
    Input,
    Spinner,
} from "@fluentui/react-components";
import {DismissRegular} from "@fluentui/react-icons";
import {authApi} from "../../shared/api/auth/authApi";
import {getUserMessageFromUnknown} from "../../shared/api/errors/getUserMessageFromUnknown";
import {validateEmail} from "../../utility/validate";

interface EmailVerificationModalProps {
    isOpen: boolean;
    onClose: () => void;
    currentEmail?: string;
    currentEmailConfirmed?: boolean;
    onSuccess: (email: string) => void;
}

type Step = "input" | "sent";

export const EmailVerificationModal: FC<EmailVerificationModalProps> = ({
                                                                            isOpen,
                                                                            onClose,
                                                                            currentEmail = "",
                                                                            currentEmailConfirmed = false,
                                                                            onSuccess,
                                                                        }) => {
    const [step, setStep] = useState<Step>("input");
    const [email, setEmail] = useState(currentEmail);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [validationError, setValidationError] = useState<string>("");
    const [callbackUrl, setCallbackUrl] = useState<string | null>(null);
    const [sentEmail, setSentEmail] = useState<string>("");

    useEffect(() => {
        if (!isOpen) return;
        setStep("input");
        setEmail(currentEmail);
        setError(null);
        setValidationError("");
        setCallbackUrl(null);
        setSentEmail("");
    }, [isOpen, currentEmail]);

    const hasMockLink = useMemo(() => Boolean(callbackUrl), [callbackUrl]);

    const parseCallbackParams = (url: string) => {
        const parsed = new URL(url, window.location.origin);
        return {
            userId: parsed.searchParams.get("userId") || "",
            token: (parsed.searchParams.get("token") || "").replace(/ /g, "+"),
            newEmail: parsed.searchParams.get("newEmail") || "",
        };
    };

    const handleSendLink = async () => {
        setError(null);
        setValidationError("");

        if (currentEmailConfirmed && email.trim() && email.trim() === currentEmail.trim()) {
            setValidationError("Этот email уже подтверждён.");
            return;
        }

        const validation = validateEmail(email);
        if (validation) {
            setValidationError(validation);
            return;
        }

        setLoading(true);
        try {
            const response = await authApi.requestEmailChange(email.trim());
            if (response.status === 429) {
                const retryAfter = response.headers.retryAfter ?? 60;
                setError(`Слишком много запросов. Подождите ${retryAfter} секунд.`);
                return;
            }

            setCallbackUrl(response.data?.callbackUrl ?? null);
            setSentEmail(email.trim());
            setStep("sent");
        } catch (err: unknown) {
            setError(getUserMessageFromUnknown(err) || "Ошибка при отправке письма. Попробуйте позже.");
        } finally {
            setLoading(false);
        }
    };

    const handleBack = () => {
        setStep("input");
        setError(null);
        setValidationError("");
    };

    const handleAssumeVerified = () => {
        setError(null);
        setValidationError("");

        if (currentEmailConfirmed && email.trim() && email.trim() === currentEmail.trim()) {
            setValidationError("Этот email уже подтверждён.");
            setStep("input");
            return;
        }

        const validation = validateEmail(email);
        if (validation) {
            setValidationError(validation);
            setStep("input");
            return;
        }

        if (!callbackUrl) {
            setError("Подтвердите email по ссылке из письма.");
            return;
        }

        void (async () => {
            setLoading(true);
            try {
                const {userId, token, newEmail} = parseCallbackParams(callbackUrl);
                if (!userId || !token || !newEmail) {
                    setError("Некорректная ссылка подтверждения");
                    return;
                }

                const r = await authApi.confirmEmailChange(userId, newEmail, token);
                if (r.error) {
                    setError(r.error);
                    return;
                }

                onSuccess(email.trim());
                onClose();
            } catch (err: unknown) {
                setError(getUserMessageFromUnknown(err) || "Ошибка подтверждения email");
            } finally {
                setLoading(false);
            }
        })();
    };

    return (
        <Dialog open={isOpen} modalType="alert" unmountOnClose={false}>
            <DialogSurface>
                <DialogBody>
                    <DialogTitle
                        action={
                            <Button
                                appearance="subtle"
                                aria-label="close"
                                icon={<DismissRegular/>}
                                onClick={onClose}
                            />
                        }
                    >
                        {step === "input" ? "Подтверждение email" : "Подтверждение по ссылке"}
                    </DialogTitle>

                    <DialogContent>
                        {step === "input" ? (
                            <>
                                <Body1 block>
                                    Введите email. Подтверждение происходит по ссылке из письма.
                                </Body1>

                                <Field
                                    label="Email"
                                    required
                                    validationMessage={validationError || error || undefined}
                                    validationState={validationError || error ? "error" : "none"}
                                >
                                    <Input
                                        type="email"
                                        value={email}
                                        onChange={(e) => {
                                            setEmail(e.target.value);
                                            setValidationError("");
                                            setError(null);
                                        }}
                                        placeholder="name@example.com"
                                        disabled={loading}
                                    />
                                </Field>
                            </>
                        ) : (
                            <>
                                <Body1 block>
                                    Мы отправили ссылку на email <strong>{sentEmail || email}</strong>
                                </Body1>

                                <Caption1 block>
                                    Откройте письмо и перейдите по ссылке, чтобы подтвердить смену email.
                                </Caption1>

                                {error && (
                                    <Caption1 block>
                                        {error}
                                    </Caption1>
                                )}
                            </>
                        )}
                    </DialogContent>

                    <DialogActions position="end">
                        {step === "input" ? (
                            <>
                                <Button appearance="secondary" onClick={onClose} disabled={loading}>
                                    Отмена
                                </Button>
                                <Button
                                    appearance="primary"
                                    onClick={handleSendLink}
                                    disabled={
                                        loading ||
                                        !email.trim()
                                    }
                                >
                                    {loading ? <Spinner size="tiny"/> : "Отправить ссылку"}
                                </Button>
                            </>
                        ) : (
                            <>
                                <Button appearance="secondary" onClick={handleBack} disabled={loading}>
                                    Изменить email
                                </Button>
                                <Button
                                    appearance="primary"
                                    onClick={handleAssumeVerified}
                                    disabled={loading || !hasMockLink}
                                >
                                    {loading ? <Spinner size="tiny"/> : "Я подтвердил"}
                                </Button>
                            </>
                        )}
                    </DialogActions>
                </DialogBody>
            </DialogSurface>
        </Dialog>
    );
};
