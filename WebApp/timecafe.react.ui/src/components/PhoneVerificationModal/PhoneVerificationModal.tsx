import {useState, useEffect, useRef, type FC} from "react";
import ReCAPTCHA from "react-google-recaptcha";
import {
    Dialog,
    DialogSurface,
    DialogTitle,
    DialogBody,
    DialogActions,
    DialogContent,
    Button,
    Input,
    Field,
    Body1,
    Caption1,
    Spinner,
} from "@fluentui/react-components";
import {DismissRegular} from "@fluentui/react-icons";
import {SendPhoneConfirmation, VerifyPhoneConfirmation} from "../../api/auth";
import type {PhoneCodeRequest} from "../../api/auth";
import {parseErrorMessage, handleVerificationError} from "../../utility/errors";
import {validatePhoneNumber} from "../../utility/validate";
import {useRateLimitedRequest} from "../../hooks/useRateLimitedRequest.ts";


interface PhoneVerificationModalProps {
    isOpen: boolean;
    onClose: () => void;
    currentPhoneNumber?: string;
    onSuccess: (phoneNumber: string) => void;
}

type Step = "input" | "verify";

export const PhoneVerificationModal: FC<PhoneVerificationModalProps> = ({
                                                                            isOpen,
                                                                            onClose,
                                                                            currentPhoneNumber = "",
                                                                            onSuccess,
                                                                        }) => {
    const [step, setStep] = useState<Step>("input");
    const [phoneNumber, setPhoneNumber] = useState(currentPhoneNumber);
    const [verificationCode, setVerificationCode] = useState("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [validationError, setValidationError] = useState<string>("");
    const [remainingAttempts, setRemainingAttempts] = useState<number | null>(null);
    const [attemptsExhausted, setAttemptsExhausted] = useState(false);
    const [requiresCaptcha, setRequiresCaptcha] = useState(false);
    const [captchaToken, setCaptchaToken] = useState<string | null>(null);
    const [captchaKey, setCaptchaKey] = useState(0);

    const recaptchaRef = useRef<ReCAPTCHA>(null);
    const RECAPTCHA_SITE_KEY = import.meta.env.VITE_RECAPTCHA_SITE_KEY || "";

    const {countdown, isBlocked, sendRequest} = useRateLimitedRequest(
        'sms-verification',
        async () => {
            const response = await SendPhoneConfirmation({phoneNumber, code: ""});
            return response;
        }
    );

    const resetVerificationState = () => {
        setRemainingAttempts(null);
        setAttemptsExhausted(false);
        setRequiresCaptcha(false);
        setCaptchaToken(null);
        setVerificationCode("");
    };

    const resetErrors = () => {
        setError(null);
        setValidationError("");
    };

    const resetCaptcha = () => {
        setCaptchaToken(null);
        setCaptchaKey(prev => prev + 1);
        if (recaptchaRef.current) {
            recaptchaRef.current.reset();
        }
    };

    useEffect(() => {
        if (isOpen) {
            setPhoneNumber(currentPhoneNumber);
            resetVerificationState();
            resetErrors();
            setStep("input");
        }
    }, [isOpen, currentPhoneNumber]);

    const handleSendCode = async () => {
        resetErrors();

        const validation = validatePhoneNumber(phoneNumber);
        if (validation) {
            setValidationError(validation);
            return;
        }

        if (isBlocked) {
            return;
        }

        setLoading(true);

        try {
            await sendRequest();
            resetVerificationState();
            setStep("verify");
        } catch (err: unknown) {
            if (typeof err === 'object' && err !== null && 'status' in err && err.status === 429) {
                const retryAfter = 'retryAfter' in err ? (err.retryAfter as number) : 60;
                setError(`Слишком много запросов. Подождите ${retryAfter} секунд.`);
                return;
            }
            setError(parseErrorMessage(err) || "Ошибка при отправке SMS. Попробуйте позже.");
        } finally {
            setLoading(false);
        }
    };

    const handleVerifyCode = async () => {
        resetErrors();

        if (requiresCaptcha && !captchaToken) {
            setError("Пройдите проверку капчи");
            return;
        }

        setLoading(true);

        try {
            const data: PhoneCodeRequest = {
                phoneNumber,
                code: verificationCode,
                captchaToken: captchaToken || undefined,
            };

            await VerifyPhoneConfirmation(data);

            resetVerificationState();

            onSuccess(phoneNumber);
            onClose();
        } catch (err: unknown) {
            const {
                errorMessage,
                remainingAttempts: attempts,
                attemptsExhausted: exhausted,
                requiresCaptcha: needsCaptcha
            } = handleVerificationError(err);

            setError(errorMessage);
            setRemainingAttempts(attempts);
            setAttemptsExhausted(exhausted);

            if (needsCaptcha) {
                setRequiresCaptcha(true);
                resetCaptcha();
            } else {
                setRequiresCaptcha(false);
            }
        } finally {
            setLoading(false);
        }
    };
    const handleResendCode = async () => {
        resetVerificationState();
        resetErrors();
        resetCaptcha();
        await handleSendCode();
    };

    const handleBack = () => {
        setStep("input");
        setVerificationCode("");
        resetErrors();
    };

    const formatCountdown = (seconds: number): string => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return mins > 0 ? `${mins}:${secs.toString().padStart(2, "0")}` : `${secs} сек`;
    };

    return (
        <Dialog open={isOpen} onOpenChange={(_, data) => !data.open && onClose()}>
            <DialogSurface className="phone-verification-modal">
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
                        {step === "input" ? "Подтверждение номера телефона" : "Введите код из SMS"}
                    </DialogTitle>
                    <DialogContent>
                        {step === "input" ? (
                            <>
                                <Body1>
                                    Введите номер телефона, на который будет отправлен код подтверждения
                                </Body1>
                                <Field
                                    label="Номер телефона"
                                    required
                                    validationMessage={validationError || error || undefined}
                                    validationState={validationError || error ? "error" : "none"}
                                >
                                    <Input
                                        type="tel"
                                        value={phoneNumber}
                                        onChange={(e) => {
                                            setPhoneNumber(e.target.value);
                                            setValidationError("");
                                            setError(null);
                                        }}
                                        placeholder="+7 (999) 123-45-67"
                                        disabled={loading}
                                    />
                                </Field>
                            </>
                        ) : (
                            <>
                                <Body1>
                                    Код отправлен на номер <strong>{phoneNumber}</strong>
                                </Body1>
                                <Field
                                    label="Код подтверждения"
                                    required
                                    validationMessage={error}
                                    validationState={error ? "error" : "none"}
                                    hint={
                                        remainingAttempts !== null && remainingAttempts > 0
                                            ? `Введите 6-значный код из SMS (осталось попыток: ${remainingAttempts})`
                                            : "Введите 6-значный код из SMS"
                                    }
                                >
                                    <Input
                                        type="text"
                                        value={verificationCode}
                                        onChange={(e) => {
                                            const val = e.target.value.replace(/\D/g, "").slice(0, 6);
                                            setVerificationCode(val);
                                            setError(null);
                                        }}
                                        placeholder="000000"
                                        maxLength={6}
                                        disabled={loading || attemptsExhausted}
                                        autoFocus
                                    />
                                </Field>

                                {RECAPTCHA_SITE_KEY && (
                                    <div style={{display: requiresCaptcha ? 'block' : 'none'}}>
                                        <ReCAPTCHA
                                            key={captchaKey}
                                            ref={recaptchaRef}
                                            sitekey={RECAPTCHA_SITE_KEY}
                                            onChange={(token) => setCaptchaToken(token)}
                                            onExpired={() => setCaptchaToken(null)}
                                            onErrored={() => {
                                                setCaptchaToken(null);
                                                setError("Ошибка загрузки капчи. Попробуйте обновить страницу.");
                                            }}
                                        />
                                    </div>
                                )}


                            </>
                        )}
                    </DialogContent>

                    {step === "verify" && (
                        <DialogActions position="start">
                            {isBlocked ? (
                                <Caption1>
                                    Отправить повторно через {formatCountdown(countdown)}
                                </Caption1>
                            ) : (
                                <Button
                                    appearance="subtle"
                                    onClick={handleResendCode}
                                    disabled={loading}>
                                    Отправить код повторно
                                </Button>
                            )}
                        </DialogActions>
                    )}

                    <DialogActions position="end">
                        {step === "input" ? (
                            <>
                                <Button appearance="secondary" onClick={onClose} disabled={loading}>
                                    Отмена
                                </Button>
                                <Button
                                    appearance="primary"
                                    onClick={handleSendCode}
                                    disabled={loading || !phoneNumber.trim() || isBlocked}
                                >
                                    {loading ? <Spinner
                                        size="tiny"/> : countdown > 0 ? `Получить код (${formatCountdown(countdown)})` : "Получить код"}
                                </Button>
                            </>
                        ) : (
                            <>
                                <Button
                                    appearance="secondary"
                                    onClick={handleBack}
                                    disabled={loading || (isBlocked && !attemptsExhausted)}
                                >
                                    {isBlocked && !attemptsExhausted ? `Изменить (${formatCountdown(countdown)})` : "Изменить номер"}
                                </Button>
                                <Button
                                    appearance="primary"
                                    onClick={handleVerifyCode}
                                    disabled={loading || verificationCode.length !== 6 || attemptsExhausted}
                                >
                                    {loading ? <Spinner size="tiny"/> : "Подтвердить"}
                                </Button>
                            </>
                        )}
                    </DialogActions>
                </DialogBody>
            </DialogSurface>
        </Dialog>
    );
};
