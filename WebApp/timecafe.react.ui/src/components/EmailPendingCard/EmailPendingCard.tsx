import {Card, Subtitle1, Field, Input, Button, MessageBar} from '@fluentui/react-components';
import {useState} from 'react';
import {useSelector} from 'react-redux';
import type {RootState} from '../../store';
import {resendConfirmation} from '../../api/auth';
import {useProgressToast} from '../ToastProgress/ToastProgress';
import {useRateLimitedRequest} from '../../hooks/useRateLimitedRequest';
import {MockCallbackLink} from '../MockCallbackLink/MockCallbackLink';

interface EmailPendingCardProps {
    showResend?: boolean
    onGoToLogin?: () => void
    mockLink?: string
}

export function EmailPendingCard({onGoToLogin, mockLink}: EmailPendingCardProps) {
    const {showToast, ToasterElement} = useProgressToast();
    const email = useSelector((s: RootState) => s.auth.email);
    const [errors, setErrors] = useState<{ password?: string; resend?: string }>({});
    const USE_MOCK_EMAIL = import.meta.env.VITE_USE_MOCK_EMAIL === 'true';
    const [internalMockLink, setInternalMockLink] = useState<string | undefined>(mockLink);

    const resend = useRateLimitedRequest(`email_resend_${email}`, async () => {
        const r = await resendConfirmation(email);
        return {data: r.data, headers: r.headers, status: r.status};
    });

    const handleResend = async () => {
        setErrors({});
        try {
            const d = await resend.sendRequest();
            if (d?.callbackUrl && USE_MOCK_EMAIL) setInternalMockLink(d.callbackUrl);
            else showToast(d?.message || 'Письмо отправлено', 'success', 'OK');
        } catch (e: any) {
            if (e?.errors?.email) setErrors({resend: e.errors.email});
            else showToast('Ошибка повторной отправки', 'error', 'Ошибка');
        }
    };

    return (
        <Card className="auth_card">
            {ToasterElement}
            <MockCallbackLink url={internalMockLink}/>
            <Subtitle1 align="center">Подтвердите email</Subtitle1>
            <Field label="Почта"
                   hint={resend.remaining > 0 && !resend.isBlocked ? `Осталось попыток: ${resend.remaining}` : undefined}>
                <Input value={email} readOnly/>
            </Field>

            {errors.resend && <MessageBar intent="error">{errors.resend}</MessageBar>}

            <div className="button-action">
                {onGoToLogin && <Button as="a" appearance="outline" onClick={onGoToLogin}>Перейти к входу</Button>}
                <Button appearance="primary" disabled={resend.isLoading || resend.isBlocked}
                        onClick={handleResend}>
                    {resend.isBlocked ? `Подождите ${resend.countdown} сек` : resend.isLoading ? 'Отправка...' : 'Отправить'}
                </Button>

            </div>
        </Card>
    );
}
