import {useEffect, useState} from 'react';
import {useSearchParams, useNavigate} from 'react-router-dom';
import {Card, Subtitle1, Spinner, Button, Field} from '@fluentui/react-components';
import {confirmEmail} from '../api/auth';
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";

export default function ConfirmEmailPage() {
    const [search] = useSearchParams();
    const {ToasterElement} = useProgressToast();
    const navigate = useNavigate();
    const userId = search.get('userId') || '';
    const token = search.get('token') || '';
    const [status, setStatus] = useState<'pending' | 'success' | 'error'>('pending');
    const [message, setMessage] = useState('');

    useEffect(() => {
        const run = async () => {
            if (!userId || !token) {
                setStatus('error');
                setMessage('Параметры отсутствуют');
                return;
            }
            const res = await confirmEmail(userId, token);
            if (res.error) {
                setStatus('error');
                setMessage(res.error);
            } else {
                setStatus('success');
                setMessage(res.message || 'Email подтвержден');
            }
        };
        run();
    }, [userId, token]);

    const goLogin = () => navigate('/login', {replace: true});

    return (
        <Card className='auth_card'>
            {ToasterElement}
            <Subtitle1 align='center'>Подтверждение Email</Subtitle1>
            {status === 'pending' && <Spinner size='large'/>}
            {status !== 'pending' && (
                <Field>
                    <div style={{textAlign: 'center', margin: '16px 0'}}>{message}</div>
                    <Button appearance='primary' onClick={goLogin}>Войти</Button>
                </Field>
            )}
        </Card>
    );
}
