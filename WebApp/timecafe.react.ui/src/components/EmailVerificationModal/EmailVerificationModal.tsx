import {useEffect, useState, type FC} from "react";
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

    useEffect(() => {
        if (!isOpen) return;
        setStep("input");
        setEmail(currentEmail);
        setError(null);
        setValidationError("");
    }, [isOpen, currentEmail]);

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
            setStep("sent");
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

        onSuccess(email.trim());
        onClose();
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
                                    Мы отправили ссылку на email <strong>{email}</strong>
                                </Body1>

                                <Caption1 block>
                                    В UI-режиме можно продолжить, нажав «Я подтвердил».
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
                                    disabled={loading}
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
