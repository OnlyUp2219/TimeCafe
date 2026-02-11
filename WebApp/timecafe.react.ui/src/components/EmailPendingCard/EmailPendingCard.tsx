import {Badge, Subtitle1, Field, Input, MessageBar} from '@fluentui/react-components';
import {useState} from 'react';
import {Mail24Regular, MailCheckmark24Regular, Person24Regular} from '@fluentui/react-icons';
import {useSelector} from 'react-redux';
import type {RootState} from '@store';
import {authApi} from "@api/auth/authApi";
import {useProgressToast} from "@components/ToastProgress/ToastProgress";
import {useRateLimitedRequest} from '@hooks/useRateLimitedRequest';
import {MockCallbackLink} from "@components/MockCallbackLink/MockCallbackLink";
import {getUserMessageFromUnknown} from "@api/errors/getUserMessageFromUnknown";
import {TooltipButton} from "@components/TooltipButton/TooltipButton";

interface EmailPendingCardProps {
    showResend?: boolean
    onGoToLogin?: () => void
    mockLink?: string
}

export function EmailPendingCard({onGoToLogin, mockLink}: EmailPendingCardProps) {
    const {showToast, ToasterElement} = useProgressToast();
    const email = useSelector((s: RootState) => s.auth.email);
    const USE_MOCK_EMAIL = import.meta.env.VITE_USE_MOCK_EMAIL === 'true';
    const [internalMockLink, setInternalMockLink] = useState<string | undefined>(mockLink);


    const [successSent, setSuccessSent] = useState(false);

    const [errors, setErrors] = useState<{ resend?: string; general?: string }>({});

    const resend = useRateLimitedRequest(`email_resend_${email}`, async () => {
        const r = await authApi.resendConfirmation(email);
        return {data: r.data, headers: r.headers, status: r.status};
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
            setErrors({general: getUserMessageFromUnknown(e) || 'Ошибка повторной отправки'});
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
        <div className="w-full">
            {ToasterElement}
            <div className="email-pending_card__header" aria-live="polite">
                <Badge appearance="tint" shape="rounded" size="extra-large" className="dark-green">
                    {successSent ? <MailCheckmark24Regular/> : <Mail24Regular/>}
                </Badge>
                <Subtitle1 align="center" className="font-semibold">Подтвердите email</Subtitle1>
                <p className="email-pending_card__text">
                    Мы отправили письмо на <span className="font-medium">{email}</span>.<br/>
                    Откройте его и перейдите по ссылке для завершения регистрации.
                    <br/>Если письмо долго не приходит — проверьте Спам или нажмите повторно.
                </p>
            </div>

            <Field label="Почта" hint={showAttemptsHint ? `Осталось попыток: ${resend.remaining}` : undefined}
                   validationState={errors.resend ? 'error' : undefined}
                   validationMessage={errors.resend}
            >
                <Input value={email} readOnly/>
            </Field>

            {canOpenMockLink && (
                <div className="email-pending_card__mocklink">
                    <MockCallbackLink url={internalMockLink}/>
                </div>
            )}

            {errors.general && (
                <MessageBar intent="error">
                    {errors.general}
                </MessageBar>
            )}

            <div className="grid grid-cols-1 gap-[12px] sm:grid-cols-2">
                {canOpenMockLink && (
                    <TooltipButton
                        appearance="primary"
                        onClick={() => window.open(internalMockLink, "_blank")}
                        tooltip="Открыть mock-ссылку из письма"
                        label="Открыть ссылку"
                        className="w-full order-1 sm:order-2"
                    />
                )}

                {onGoToLogin && (
                    <TooltipButton
                        appearance="secondary"
                        onClick={onGoToLogin}
                        icon={<Person24Regular/>}
                        tooltip="Перейти на страницу входа"
                        label="Перейти к входу"
                        className={canOpenMockLink ? "w-full order-2 sm:order-1" : "w-full"}
                    />
                )}
                <TooltipButton
                    appearance="primary"
                    disabled={resend.isLoading || resend.isBlocked}
                    onClick={handleResend}
                    tooltip={resend.isBlocked ? "Подождите перед повторной отправкой" : "Отправить письмо ещё раз"}
                    label={buttonLabel}
                    className={canOpenMockLink ? "w-full order-3 sm:order-3" : "w-full"}
                />
            </div>
        </div>
    );
}
