import {type FC, useEffect, useRef, useState} from "react";
import ReCAPTCHA from "react-google-recaptcha";
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
import {authApi, type PhoneCodeRequest} from "../../shared/api/auth/authApi";
import {getUserMessageFromUnknown} from "../../shared/api/errors/getUserMessageFromUnknown";
import {handleVerificationError} from "../../shared/auth/phoneVerification";
import {validatePhoneNumber} from "../../utility/validate";
import {useRateLimitedRequest} from "../../hooks/useRateLimitedRequest.ts";

type PhoneVerificationSessionV1 = {
    open: boolean;
    step: Step;
    phoneNumber: string;
    mode: "api" | "ui";
    uiGeneratedCode?: string | null;
};

const PHONE_VERIFICATION_SESSION_KEY = "tc_phone_verification_session_v1";

const loadPhoneSession = (): PhoneVerificationSessionV1 | null => {
    try {
        const raw = window.localStorage.getItem(PHONE_VERIFICATION_SESSION_KEY);
        if (!raw) return null;
        const parsed = JSON.parse(raw) as PhoneVerificationSessionV1;
        if (!parsed || typeof parsed !== "object") return null;
        if (parsed.step !== "verify") return null;
        if (parsed.mode !== "api" && parsed.mode !== "ui") return null;
        return {
            open: Boolean(parsed.open),
            step: parsed.step,
            phoneNumber: String(parsed.phoneNumber ?? ""),
            mode: parsed.mode,
            uiGeneratedCode: parsed.uiGeneratedCode ?? null,
        };
    } catch {
        return null;
    }
};

const savePhoneSession = (session: PhoneVerificationSessionV1) => {
    try {
        window.localStorage.setItem(PHONE_VERIFICATION_SESSION_KEY, JSON.stringify(session));
    } catch {
        void 0;
    }
};

const clearPhoneSession = () => {
    try {
        window.localStorage.removeItem(PHONE_VERIFICATION_SESSION_KEY);
    } catch {
        void 0;
    }
};


interface PhoneVerificationModalProps {
    isOpen: boolean;
    onClose: () => void;
    currentPhoneNumber?: string;
    currentPhoneNumberConfirmed?: boolean;
    onSuccess: (phoneNumber: string) => void;
    mode?: "api" | "ui";
}

type Step = "input" | "verify";

export const PhoneVerificationModal: FC<PhoneVerificationModalProps> = ({
                                                                            isOpen,
                                                                            onClose,
                                                                            currentPhoneNumber = "",
                                                                            currentPhoneNumberConfirmed = false,
                                                                            onSuccess,
                                                                            mode = "api",
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
    const [uiGeneratedCode, setUiGeneratedCode] = useState<string | null>(null);

    const recaptchaRef = useRef<ReCAPTCHA>(null);
    const RECAPTCHA_SITE_KEY = import.meta.env.VITE_RECAPTCHA_SITE_KEY || "";

    const {countdown, isBlocked, sendRequest} = useRateLimitedRequest(
        'sms-verification',
        async () => {
            return await authApi.sendPhoneConfirmation({phoneNumber, code: ""});
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
        if (!isOpen) return;

        const session = loadPhoneSession();
        if (session?.open && session.mode === mode && session.phoneNumber.trim()) {
            setPhoneNumber(session.phoneNumber);
            resetVerificationState();
            resetErrors();
            setStep("verify");
            setUiGeneratedCode(mode === "ui" ? (session.uiGeneratedCode ?? null) : null);
            return;
        }

        setPhoneNumber(currentPhoneNumber);
        resetVerificationState();
        resetErrors();
        setStep("input");
        setUiGeneratedCode(null);
    }, [isOpen, currentPhoneNumber, mode]);

    useEffect(() => {
        if (!isOpen) return;
        if (step !== "verify") {
            clearPhoneSession();
            return;
        }
        savePhoneSession({
            open: true,
            step: "verify",
            phoneNumber,
            mode,
            uiGeneratedCode: mode === "ui" ? uiGeneratedCode : null,
        });
    }, [isOpen, mode, phoneNumber, step, uiGeneratedCode]);

    const handleClose = () => {
        clearPhoneSession();
        onClose();
    };

    const handleSendCode = async () => {
        resetErrors();

        if (
            currentPhoneNumberConfirmed && phoneNumber.trim() &&
            phoneNumber.trim() === currentPhoneNumber.trim()
        ) {
            setValidationError("Этот номер уже подтверждён.");
            return;
        }

        const validation = validatePhoneNumber(phoneNumber);
        if (validation) {
            setValidationError(validation);
            return;
        }

        if (isBlocked) {
            return;
        }

        if (mode === "ui") {
            const code = String(Math.floor(100000 + Math.random() * 900000));
            setUiGeneratedCode(code);
            setStep("verify");
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
            setError(getUserMessageFromUnknown(err) || "Ошибка при отправке SMS. Попробуйте позже.");
        } finally {
            setLoading(false);
        }
    };

    const handleVerifyCode = async () => {
        resetErrors();

        if (mode === "ui") {
            const safeCode = (verificationCode || "").replace(/\D/g, "");
            if (!uiGeneratedCode) {
                setError("Сначала запросите код подтверждения");
                return;
            }
            if (safeCode !== uiGeneratedCode) {
                setError("Неверный код подтверждения");
                return;
            }

            resetVerificationState();
            setUiGeneratedCode(null);
            onSuccess(phoneNumber);
            handleClose();
            return;
        }

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

            await authApi.verifyPhoneConfirmation(data);

            resetVerificationState();

            onSuccess(phoneNumber);
            handleClose();
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
        setUiGeneratedCode(null);
        await handleSendCode();
    };

    const handleBack = () => {
        setStep("input");
        setVerificationCode("");
        resetErrors();
        setUiGeneratedCode(null);
    };

    const formatCountdown = (seconds: number): string => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return mins > 0 ? `${mins}:${secs.toString().padStart(2, "0")}` : `${secs} сек`;
    };

    return (
        <Dialog open={isOpen} onOpenChange={(_, data) => !data.open && handleClose()}>
            <DialogSurface className="phone-verification-modal">
                <DialogBody>
                    <DialogTitle
                        action={
                            <Button
                                appearance="subtle"
                                aria-label="close"
                                icon={<DismissRegular/>}
                                onClick={handleClose}
                            />
                        }
                    >
                        {step === "input" ? "Подтверждение номера телефона" : "Введите код из SMS"}
                    </DialogTitle>
                    <DialogContent>
                        {step === "input" ? (
                            <>
                                <Body1 block>
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
                                <Body1 block>
                                    Код отправлен на номер <strong>{phoneNumber}</strong>
                                </Body1>
                                {mode === "ui" && uiGeneratedCode && (
                                    <Caption1 block>
                                        UI-режим: код подтверждения — <strong>{uiGeneratedCode}</strong>
                                    </Caption1>
                                )}
                                <Field
                                    label="Код подтверждения"
                                    required
                                    validationMessage={error}
                                    validationState={error ? "error" : "none"}
                                    hint={
                                        remainingAttempts !== null && remainingAttempts > 0
                                            ? `Введите 6-значный код (осталось попыток: ${remainingAttempts})`
                                            : "Введите 6-значный код"
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

                                {RECAPTCHA_SITE_KEY && requiresCaptcha && (
                                    <div>
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
                                <Button appearance="secondary" onClick={handleClose} disabled={loading}>
                                    Отмена
                                </Button>
                                <Button
                                    appearance="primary"
                                    onClick={handleSendCode}
                                    disabled={
                                        loading ||
                                        !phoneNumber.trim() ||
                                        isBlocked
                                    }
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
                                    disabled={
                                        loading ||
                                        verificationCode.length !== 6 ||
                                        attemptsExhausted
                                    }
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
