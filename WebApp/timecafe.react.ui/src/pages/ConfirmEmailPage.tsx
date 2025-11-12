import {useEffect, useState} from 'react';
import {useSearchParams, useNavigate} from 'react-router-dom';
import {Card, Title3, Body1, Spinner, Button} from '@fluentui/react-components';
import {CheckmarkCircle24Filled, DismissCircle24Filled, MailCheckmark24Regular} from '@fluentui/react-icons';
import {confirmEmail} from '../api/auth';
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";

export default function ConfirmEmailPage() {
    const [search] = useSearchParams();
    const {ToasterElement} = useProgressToast();
    const navigate = useNavigate();
    const userId = search.get('userId') || '';
    const token = search.get('token') || '';
    const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
    const [message, setMessage] = useState('');

    useEffect(() => {
        const run = async () => {
            if (!userId || !token) {
                setStatus('error');
                setMessage('Ссылка недействительна или повреждена');
                return;
            }
            const res = await confirmEmail(userId, token);
            if (res.error) {
                setStatus('error');
                setMessage(res.error);
            } else {
                setStatus('success');
                setMessage(res.message || 'Email успешно подтвержден');
            }
        };
        run();
    }, [userId, token]);

    const goLogin = () => navigate('/login', {replace: true});

    if (status === 'loading') {
        return (
            <Card className='auth_card status-card'>
                {ToasterElement}
                <div className="status-icon status-icon--pending">
                    <MailCheckmark24Regular/>
                </div>
                <Title3>Подтверждение Email</Title3>
                <Spinner size="large" label="Проверяем ваш email..."/>
            </Card>
        );
    }

    return (
        <Card className='auth_card status-card '>
            {ToasterElement}

            {status === 'success' && (
                <>
                    <div className="status-icon status-icon--success">
                        <CheckmarkCircle24Filled/>
                    </div>
                    <Title3>Email подтвержден! 🎉</Title3>
                    <Body1 className="status-message">
                        {message}
                        <br/>
                        <span className="inline-block">Теперь вы можете войти в систему и пользоваться всеми функциями TimeCafe.</span>
                    </Body1>
                    <Button appearance="primary" onClick={goLogin} className="w-full">
                        Войти в систему
                    </Button>
                </>
            )}

            {status === 'error' && (
                <>
                    <div className="status-icon status-icon--error">
                        <DismissCircle24Filled/>
                    </div>
                    <Title3>Ошибка подтверждения</Title3>
                    <Body1 className="status-message">
                        {message}
                        <br/>
                        <span className="inline-block">
                            {message.includes('уже подтвержден') 
                                ? 'Попробуйте войти в систему.' 
                                : 'Попробуйте войти снова и запросить новое письмо подтверждения.'}
                        </span>
                    </Body1>
                    <Button appearance="primary" onClick={goLogin} className="w-full">
                        Перейти к входу
                    </Button>
                </>
            )}
        </Card>
    );
}
