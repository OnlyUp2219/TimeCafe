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
import {
    isPhoneVerificationSessionV1,
    PHONE_VERIFICATION_SESSION_KEY,
    type PhoneVerificationSessionV1,
} from "../../shared/auth/phoneVerificationSession";
import {validatePhoneNumber} from "../../utility/validate";
import {useRateLimitedRequest} from "../../hooks/useRateLimitedRequest.ts";
import {useLocalStorageJson} from "../../hooks/useLocalStorageJson";
import {PhoneInput} from "../FormFields";


interface PhoneVerificationModalProps {
    isOpen: boolean;
    onClose: () => void;
    currentPhoneNumber?: string;
    currentPhoneNumberConfirmed?: boolean;
    onSuccess: (phoneNumber: string) => void;
    mode?: "api" | "ui";
    autoSendCodeOnOpen?: boolean;
}

type Step = "input" | "verify";

export const PhoneVerificationModal: FC<PhoneVerificationModalProps> = ({
                                                                            isOpen,
                                                                            onClose,
                                                                            currentPhoneNumber = "",
                                                                            currentPhoneNumberConfirmed = false,
                                                                            onSuccess,
                                                                            mode = "api",
                                                                            autoSendCodeOnOpen = false,
                                                                        }) => {
    const phoneSessionStore = useLocalStorageJson<PhoneVerificationSessionV1>(
        PHONE_VERIFICATION_SESSION_KEY,
        isPhoneVerificationSessionV1
    );

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
    const [mockGeneratedCode, setMockGeneratedCode] = useState<string | null>(null);
    const [autoSendRequested, setAutoSendRequested] = useState(false);
    const autoSendOnceRef = useRef(false);

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

        autoSendOnceRef.current = false;

        const session = phoneSessionStore.load();
        if (session?.open && session.step === "verify" && session.mode === mode && session.phoneNumber.trim()) {
            setPhoneNumber(session.phoneNumber);
            resetVerificationState();
            resetErrors();
            setStep("verify");
            setUiGeneratedCode(mode === "ui" ? (session.uiGeneratedCode ?? null) : null);
            setAutoSendRequested(false);
            return;
        }

        setPhoneNumber(currentPhoneNumber);
        resetVerificationState();
        resetErrors();
        setStep("input");
        setUiGeneratedCode(null);
        setMockGeneratedCode(null);
        setAutoSendRequested(autoSendCodeOnOpen && Boolean(currentPhoneNumber.trim()));
    }, [isOpen, currentPhoneNumber, mode]);

    useEffect(() => {
        if (!isOpen) return;
        if (!autoSendRequested) return;
        if (step !== "input") {
            setAutoSendRequested(false);
            return;
        }

        if (autoSendOnceRef.current) {
            setAutoSendRequested(false);
            return;
        }
        autoSendOnceRef.current = true;

        setAutoSendRequested(false);
        void handleSendCode();
    }, [autoSendRequested, isOpen, step]);

    useEffect(() => {
        if (!isOpen) return;
        if (step !== "verify") {
            phoneSessionStore.clear();
            return;
        }
        phoneSessionStore.save({
            open: true,
            step,
            phoneNumber,
            mode,
            uiGeneratedCode: mode === "ui" ? uiGeneratedCode : null,
        });
    }, [isOpen, mode, phoneNumber, step, uiGeneratedCode]);

    const handleClose = () => {
        phoneSessionStore.clear();
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
            const response = await sendRequest();
            resetVerificationState();
            setMockGeneratedCode(response?.token ?? null);
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
            setMockGeneratedCode(null);
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
            setMockGeneratedCode(null);

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
        setMockGeneratedCode(null);
        await handleSendCode();
    };

    const handleBack = () => {
        setStep("input");
        setVerificationCode("");
        resetErrors();
        setUiGeneratedCode(null);
        setMockGeneratedCode(null);
    };

    const formatCountdown = (seconds: number): string => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return mins > 0 ? `${mins}:${secs.toString().padStart(2, "0")}` : `${secs} сек`;
    };

    return (
        <Dialog open={isOpen} modalType="alert" unmountOnClose={false}>
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
                                <PhoneInput
                                    label="Номер телефона"
                                    required
                                    value={phoneNumber}
                                    onChange={(value) => {
                                        setPhoneNumber(value);
                                        setValidationError("");
                                        setError(null);
                                    }}
                                    placeholder="+7 (999) 123-45-67"
                                    disabled={loading}
                                    validateOnBlur
                                    externalError={validationError || error || undefined}
                                />
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
                                {mode === "api" && mockGeneratedCode && (
                                    <Caption1 block>
                                        Mock-режим: код подтверждения — <strong>{mockGeneratedCode}</strong>
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
