import { Badge, Title3, Body2, Field, Input, MessageBar } from "@fluentui/react-components";
import { useState } from "react";
import { Mail24Regular, MailCheckmark24Regular, Person24Regular } from "@fluentui/react-icons";
import { useAppSelector } from "@store/hooks";
import { httpClient } from "@api/httpClient";
import { withRateLimit } from "@utility/rateLimitHelper";
import { useProgressToast } from "@components/ToastProgress/ToastProgress";
import { useRateLimitedRequest } from "@hooks/useRateLimitedRequest";
import { MockCallbackLink } from "@components/MockCallbackLink/MockCallbackLink";
import { getUserMessageFromUnknown } from "@api/errors/getUserMessageFromUnknown";
import { TooltipButton } from "@components/TooltipButton/TooltipButton";
import { useComponentSize } from "@hooks/useComponentSize";

interface EmailPendingCardProps {
    showResend?: boolean
    onGoToLogin?: () => void
    mockLink?: string
}

export function EmailPendingCard({ onGoToLogin, mockLink }: EmailPendingCardProps) {
    const { showToast, ToasterElement } = useProgressToast();
    const { sizes } = useComponentSize();
    const email = useAppSelector((s) => s.auth.email);
    const USE_MOCK_EMAIL = import.meta.env.VITE_USE_MOCK_EMAIL === 'true';
    const [internalMockLink, setInternalMockLink] = useState<string | undefined>(mockLink);

    const [successSent, setSuccessSent] = useState(false);

    const [errors, setErrors] = useState<{ resend?: string; general?: string }>({});

    const resend = useRateLimitedRequest<{ message?: string; callbackUrl?: string }>(`email_resend_${email}`, async () => {
        const endpoint = USE_MOCK_EMAIL ? "/auth/email/resend-mock" : "/auth/email/resend";
        return withRateLimit(() => httpClient.post<{ message?: string; callbackUrl?: string }>(endpoint, { email }));
    });

    const handleResend = async () => {
        setErrors({});
        setSuccessSent(false);
        try {
            const d = await resend.sendRequest();
            if (d?.callbackUrl && USE_MOCK_EMAIL) setInternalMockLink(d.callbackUrl);
            showToast(d?.message || 'Письмо отправлено', 'success', 'OK');
            setSuccessSent(true);
        } catch (e: unknown) {
            setErrors({ general: getUserMessageFromUnknown(e) || 'Ошибка повторной отправки' });
        }
    };

    const buttonLabel = resend.isBlocked
        ? `Подождите ${resend.countdown} сек`
        : resend.isLoading
            ? 'Отправка...'
            : 'Отправить';

    const showAttemptsHint = resend.remaining > 0 && !resend.isBlocked;

    const canOpenMockLink = Boolean(internalMockLink) && USE_MOCK_EMAIL;

    return (
        <div className="flex flex-col gap-6 w-full">
            {ToasterElement}
            <div className="flex flex-col items-center text-center gap-4" aria-live="polite">
                <Badge appearance="tint" color="success" size="extra-large" shape="rounded">
                    {successSent ? <MailCheckmark24Regular /> : <Mail24Regular />}
                </Badge>
                <Title3 className="font-semibold">Подтвердите email</Title3>
                <Body2 className="text-(--colorNeutralForeground2) max-w-sm">
                    Мы отправили письмо на <span className="font-medium text-(--colorNeutralForeground1)">{email}</span>.
                    Откройте его и перейдите по ссылке для завершения регистрации.
                    Если письмо долго не приходит — проверьте папку «Спам» или запросите повторную отправку.
                </Body2>
            </div>

            <div className="flex flex-col gap-4">
                <Field
                    label="Почта"
                    hint={showAttemptsHint ? `Осталось попыток: ${resend.remaining}` : undefined}
                    validationState={errors.resend ? 'error' : undefined}
                    validationMessage={errors.resend}
                >
                    <Input value={email} readOnly size={sizes.input} />
                </Field>

                {canOpenMockLink && (
                    <div className="flex justify-center bg-(--colorNeutralBackground2) rounded-md">
                        <MockCallbackLink url={internalMockLink} />
                    </div>
                )}

                {errors.general && (
                    <MessageBar intent="error">
                        {errors.general}
                    </MessageBar>
                )}
            </div>

            <div className="flex flex-col sm:flex-row gap-3">
                {onGoToLogin && (
                    <TooltipButton
                        appearance="secondary"
                        onClick={onGoToLogin}
                        icon={<Person24Regular />}
                        tooltip="Перейти на страницу входа"
                        label="К входу"
                        className="flex-1"
                    />
                )}

                <TooltipButton
                    appearance="primary"
                    disabled={resend.isLoading || resend.isBlocked}
                    onClick={handleResend}
                    tooltip={resend.isBlocked ? "Подождите перед повторной отправкой" : "Отправить письмо ещё раз"}
                    label={buttonLabel}
                    className="flex-1"
                />

                {canOpenMockLink && (
                    <TooltipButton
                        appearance="primary"
                        onClick={() => globalThis.open(internalMockLink, "_blank")}
                        tooltip="Открыть mock-ссылку из письма"
                        label="Открыть ссылку"
                        className="flex-1"
                    />
                )}
            </div>
        </div>
    );
}
