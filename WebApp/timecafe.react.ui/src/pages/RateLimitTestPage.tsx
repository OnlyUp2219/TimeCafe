import React from 'react';
import {Button, Input, Field, Subtitle1, Card} from '@fluentui/react-components';
import {useRateLimitedRequest} from '../hooks/useRateLimitedRequest.ts';
import {fetchRateLimited, fetchRateLimited2} from '../api/rateLimitApi.ts';
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";

export const RateLimitTestPage: React.FC = () => {
    const {showToast, ToasterElement} = useProgressToast();

    const endpoint1 = useRateLimitedRequest('test-endpoint-1', fetchRateLimited);
    const endpoint2 = useRateLimitedRequest('test-endpoint-2', fetchRateLimited2);

    return (
        <div>
            {ToasterElement}
            <Subtitle1 align="center" style={{marginBottom: 24}}>RateLimit тест</Subtitle1>

            <div className="flex flex-wrap gap-[24px] ">
                <Card className="w-full min-w-0 flex-1 basis-[288px] p-3">
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
                        onClick={async () => {
                            try {
                                await endpoint1.sendRequest();
                            } catch (err) {
                                showToast(err instanceof Error ? err.message : String(err), "error", "Ошибка");
                            }
                        }}
                        disabled={endpoint1.isLoading || endpoint1.isBlocked}
                    >
                        {endpoint1.isLoading ? 'Загрузка...' : endpoint1.isBlocked ? `Подождите ${endpoint1.countdown} сек` : 'Отправить'}
                    </Button>
                </Card>

                <Card className="w-full min-w-0 flex-1 basis-[288px] p-3">
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
                        onClick={async () => {
                            try {
                                await endpoint2.sendRequest();
                            } catch (err) {
                                showToast(err instanceof Error ? err.message : String(err), "error", "Ошибка");
                            }
                        }}
                        disabled={endpoint2.isLoading || endpoint2.isBlocked}
                    >
                        {endpoint2.isLoading ? 'Загрузка...' : endpoint2.isBlocked ? `Подождите ${endpoint2.countdown} сек` : 'Отправить'}
                    </Button>
                </Card>
            </div>
        </div>
    );
};
