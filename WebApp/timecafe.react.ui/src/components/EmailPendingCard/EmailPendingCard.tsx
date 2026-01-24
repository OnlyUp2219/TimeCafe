import {Card, Subtitle1, Field, Input, Button, MessageBar} from '@fluentui/react-components';
import {useState} from 'react';
import {Mail24Regular, MailCheckmark24Regular, Person24Regular} from '@fluentui/react-icons';
import {useSelector} from 'react-redux';
import type {RootState} from '../../store';
import {authApi} from "../../shared/api/auth/authApi";
import {useProgressToast} from '../ToastProgress/ToastProgress';
import {useRateLimitedRequest} from '../../hooks/useRateLimitedRequest';
import {MockCallbackLink} from '../MockCallbackLink/MockCallbackLink';
import {getUserMessageFromUnknown} from "../../shared/api/errors/getUserMessageFromUnknown";

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

    return (
        <Card className="auth_card email-pending_card">
            {ToasterElement}
            <div className="email-pending_card__header" aria-live="polite">
                <div className="email-pending_card__icon">
                    {successSent ? <MailCheckmark24Regular/> : <Mail24Regular/>}
                </div>
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

            {internalMockLink && USE_MOCK_EMAIL && (
                <div className="email-pending_card__mocklink">
                    <MockCallbackLink url={internalMockLink}/>
                </div>
            )}

            {errors.general && (
                <MessageBar intent="error">
                    {errors.general}
                </MessageBar>
            )}

            <div className="button-action">
                {onGoToLogin && (
                    <Button as="a" appearance="outline" onClick={onGoToLogin} icon={<Person24Regular/>}>
                        Перейти к входу
                    </Button>
                )}
                <Button appearance="primary"
                        disabled={resend.isLoading || resend.isBlocked}
                        onClick={handleResend}
                >
                    {buttonLabel}
                </Button>
            </div>
        </Card>
    );
}
