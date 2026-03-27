import type {FC, RefObject} from "react";
import ReCAPTCHA from "react-google-recaptcha";
import {Body1, Caption1, Field, Input} from "@fluentui/react-components";

interface CodeVerificationStepProps {
    phoneNumber: string;
    verificationCode: string;
    onCodeChange: (value: string) => void;
    error: string | null;
    loading: boolean;
    attemptsExhausted: boolean;
    remainingAttempts: number | null;
    mockGeneratedCode: string | null;
    requiresCaptcha: boolean;
    captchaKey: number;
    recaptchaRef: RefObject<ReCAPTCHA | null>;
    recaptchaSiteKey: string;
    onCaptchaChange: (token: string | null) => void;
    onCaptchaError: () => void;
}

export const CodeVerificationStep: FC<CodeVerificationStepProps> = ({
    phoneNumber,
    verificationCode,
    onCodeChange,
    error,
    loading,
    attemptsExhausted,
    remainingAttempts,
    mockGeneratedCode,
    requiresCaptcha,
    captchaKey,
    recaptchaRef,
    recaptchaSiteKey,
    onCaptchaChange,
    onCaptchaError,
}) => (
    <>
        <Body1 block>
            Код отправлен на номер <strong>{phoneNumber}</strong>
        </Body1>
        {mockGeneratedCode && (
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
                    onCodeChange(val);
                }}
                placeholder="000000"
                maxLength={6}
                disabled={loading || attemptsExhausted}
                autoFocus
            />
        </Field>

        {recaptchaSiteKey && requiresCaptcha && (
            <div>
                <ReCAPTCHA
                    key={captchaKey}
                    ref={recaptchaRef}
                    sitekey={recaptchaSiteKey}
                    onChange={onCaptchaChange}
                    onExpired={() => onCaptchaChange(null)}
                    onErrored={onCaptchaError}
                />
            </div>
        )}
    </>
);
