import React from 'react';
import {Button, Input, Field, Subtitle1, Card} from '@fluentui/react-components';
import {useRateLimitedRequest} from '../hooks/useRateLimitedRequest.ts';
import {fetchRateLimited, fetchRateLimited2} from '../api/rateLimitApi.ts';

export const RateLimitTestPage: React.FC = () => {
    const endpoint1 = useRateLimitedRequest('test-endpoint-1', fetchRateLimited);
    const endpoint2 = useRateLimitedRequest('test-endpoint-2', fetchRateLimited2);

    return (
        <div style={{maxWidth: 900, margin: '0 auto'}}>
            <Subtitle1 align="center" style={{marginBottom: 24}}>RateLimit тест</Subtitle1>

            <div style={{display: 'flex', gap: 24}}>
                <Card style={{flex: 1, padding: 16}}>
                    <Subtitle1>Endpoint 1</Subtitle1>

                    <Field label="Результат">
                        <Input readOnly value={endpoint1.data ? JSON.stringify(endpoint1.data, null, 2) : '—'}/>
                    </Field>

                    <Field label="Осталось до разблокировки (сек)">
                        <Input readOnly value={endpoint1.countdown.toString()}/>
                    </Field>

                    <Field label="Осталось попыток в окне">
                        <Input readOnly value={endpoint1.remaining.toString()}/>
                    </Field>

                    <Button
                        appearance="primary"
                        onClick={endpoint1.sendRequest}
                        disabled={endpoint1.isLoading || endpoint1.isBlocked}
                        style={{marginTop: 16, width: '100%'}}
                    >
                        {endpoint1.isLoading ? 'Загрузка...' : endpoint1.isBlocked ? `Подождите ${endpoint1.countdown} сек` : 'Отправить'}
                    </Button>
                </Card>

                <Card style={{flex: 1, padding: 16}}>
                    <Subtitle1>Endpoint 2</Subtitle1>

                    <Field label="Результат">
                        <Input readOnly value={endpoint2.data ? JSON.stringify(endpoint2.data, null, 2) : '—'}/>
                    </Field>

                    <Field label="Осталось до разблокировки (сек)">
                        <Input readOnly value={endpoint2.countdown.toString()}/>
                    </Field>

                    <Field label="Осталось попыток в окне">
                        <Input readOnly value={endpoint2.remaining.toString()}/>
                    </Field>

                    <Button
                        appearance="primary"
                        onClick={endpoint2.sendRequest}
                        disabled={endpoint2.isLoading || endpoint2.isBlocked}
                        style={{marginTop: 16, width: '100%'}}
                    >
                        {endpoint2.isLoading ? 'Загрузка...' : endpoint2.isBlocked ? `Подождите ${endpoint2.countdown} сек` : 'Отправить'}
                    </Button>
                </Card>
            </div>
        </div>
    );
};
