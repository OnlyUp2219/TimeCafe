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
import {getSavedRateLimit, saveRateLimit, clearRateLimit, getRemainingTime} from "./rateLimitHelpers";


interface PhoneVerificationModalProps {
    isOpen: boolean;
    onClose: () => void;
    currentPhoneNumber?: string;
    onSuccess: (phoneNumber: string) => void;
}

type Step = "input" | "verify";

const RATE_LIMIT_SECONDS = 60;

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

    const [countdown, setCountdown] = useState(0);
    const [canResend, setCanResend] = useState(true);
    const [captchaKey, setCaptchaKey] = useState(0);

    const recaptchaRef = useRef<ReCAPTCHA>(null);
    const RECAPTCHA_SITE_KEY = import.meta.env.VITE_RECAPTCHA_SITE_KEY || "";

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
        if (countdown > 0) {
            const timer = setTimeout(() => setCountdown(countdown - 1), 1000);
            return () => clearTimeout(timer);
        } else if (countdown === 0 && !canResend) {
            setCanResend(true);
        }
    }, [countdown, canResend]);

    useEffect(() => {
        if (isOpen) {
            setPhoneNumber(currentPhoneNumber);
            resetVerificationState();
            resetErrors();

            const remainingTime = getRemainingTime(RATE_LIMIT_SECONDS);
            const saved = getSavedRateLimit();

            if (remainingTime > 0 && saved.phoneNumber === currentPhoneNumber) {
                setStep("verify");
                setCountdown(remainingTime);
                setCanResend(false);
            } else {
                setStep("input");
                setCountdown(0);
                setCanResend(true);
            }
        }
    }, [isOpen, currentPhoneNumber]);
    const handleSendCode = async () => {
        resetErrors();

        const validation = validatePhoneNumber(phoneNumber);
        if (validation) {
            setValidationError(validation);
            return;
        }

        const remainingTime = getRemainingTime(RATE_LIMIT_SECONDS);
        const saved = getSavedRateLimit();
        if (remainingTime > 0 && saved.phoneNumber === phoneNumber) {
            setStep("verify");
            setCountdown(remainingTime);
            setCanResend(false);
            return;
        }

        setLoading(true);

        try {
            const data: PhoneCodeRequest = {
                phoneNumber,
                code: "",
            };
            await SendPhoneConfirmation(data);

            saveRateLimit(phoneNumber, Date.now());
            resetVerificationState();

            setStep("verify");
            setCountdown(RATE_LIMIT_SECONDS);
            setCanResend(false);
        } catch (err: unknown) {
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

            clearRateLimit();
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
                            {!canResend ? (
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
                                    disabled={loading || !phoneNumber.trim() || countdown > 0}
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
                                    disabled={loading || (!canResend && !attemptsExhausted)}
                                >
                                    {!canResend && !attemptsExhausted ? `Изменить (${formatCountdown(countdown)})` : "Изменить номер"}
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
