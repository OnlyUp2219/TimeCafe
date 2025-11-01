import { useState } from "react";
import { Button, Card, Field, Input, Subtitle1, Text, Title1 } from "@fluentui/react-components";
import { useProgressToast } from "../components/ToastProgress/ToastProgress";
import { useErrorHandler } from "../hooks/useErrorHandler";
import { errorTestApi } from "../api/errorTest";

export const ErrorTestPage = () => {
    const { showToast, ToasterElement } = useProgressToast();
    const { fieldErrors, handleError, clearAllErrors } = useErrorHandler(showToast);
    const [lastResult, setLastResult] = useState<string>("");

    const testError = async (testFn: () => Promise<unknown>, testName: string) => {
        clearAllErrors();
        setLastResult(`Тестируем: ${testName}...`);
        
        try {
            const response = await testFn();
            setLastResult(`${testName}: Успех - ${JSON.stringify(response)}`);
        } catch (error) {
            const result = handleError(error);
            setLastResult(`${testName}: ${result.message}`);
        }
    };

    return (
        <div className="flex flex-col gap-[24px] p-[24px] max-w-[1200px] mx-auto">
            {ToasterElement}
            
            <div>
                <Title1>Тестирование обработки ошибок</Title1>
                <Text>Проверка унифицированной системы обработки ошибок</Text>
            </div>

            <Card>
                <Subtitle1>Ошибки валидации (под полями)</Subtitle1>
                <div className="flex flex-col gap-[12px] mt-[16px]">
                    <Button 
                        appearance="secondary"
                        onClick={() => testError(() => errorTestApi.testValidation(), "Validation Error")}
                    >
                        Тест: Ошибки валидации (3 поля)
                    </Button>
                    
                    <Button 
                        appearance="secondary"
                        onClick={() => testError(() => errorTestApi.testMultipleValidation(), "Multiple Validation")}
                    >
                        Тест: Множественная валидация (4 поля)
                    </Button>

                    <div className="grid grid-cols-2 gap-[16px] mt-[8px]">
                        <Field
                            label="Email"
                            validationState={fieldErrors.email ? "error" : undefined}
                            validationMessage={fieldErrors.email}
                        >
                            <Input placeholder="test@example.com" />
                        </Field>

                        <Field
                            label="Password"
                            validationState={fieldErrors.password ? "error" : undefined}
                            validationMessage={fieldErrors.password}
                        >
                            <Input type="password" placeholder="******" />
                        </Field>

                        <Field
                            label="Username"
                            validationState={fieldErrors.username ? "error" : undefined}
                            validationMessage={fieldErrors.username}
                        >
                            <Input placeholder="username" />
                        </Field>

                        <Field
                            label="First Name"
                            validationState={fieldErrors.firstName ? "error" : undefined}
                            validationMessage={fieldErrors.firstName}
                        >
                            <Input placeholder="Иван" />
                        </Field>

                        <Field
                            label="Last Name"
                            validationState={fieldErrors.lastName ? "error" : undefined}
                            validationMessage={fieldErrors.lastName}
                        >
                            <Input placeholder="Иванов" />
                        </Field>

                        <Field
                            label="Age"
                            validationState={fieldErrors.age ? "error" : undefined}
                            validationMessage={fieldErrors.age}
                        >
                            <Input type="number" placeholder="25" />
                        </Field>

                        <Field
                            label="Phone Number"
                            validationState={fieldErrors.phoneNumber ? "error" : undefined}
                            validationMessage={fieldErrors.phoneNumber}
                        >
                            <Input placeholder="+7 (999) 123-45-67" />
                        </Field>
                    </div>
                </div>
            </Card>

            <Card>
                <Subtitle1>Ошибки авторизации (Toast)</Subtitle1>
                <div className="flex flex-wrap gap-[12px] mt-[16px]">
                    <Button 
                        appearance="secondary"
                        onClick={() => testError(() => errorTestApi.testUnauthorized(), "Unauthorized")}
                    >
                        Тест: 401 Unauthorized
                    </Button>

                    <Button 
                        appearance="secondary"
                        onClick={() => testError(() => errorTestApi.testForbidden(), "Forbidden")}
                    >
                        Тест: 403 Forbidden
                    </Button>
                </div>
            </Card>

            <Card>
                <Subtitle1>Бизнес-логика и конфликты (Toast Warning)</Subtitle1>
                <div className="flex flex-wrap gap-[12px] mt-[16px]">
                    <Button 
                        appearance="secondary"
                        onClick={() => testError(() => errorTestApi.testBusinessLogic(), "Business Logic")}
                    >
                        Тест: Бизнес-логика
                    </Button>

                    <Button 
                        appearance="secondary"
                        onClick={() => testError(() => errorTestApi.testConflict(), "Conflict")}
                    >
                        Тест: 409 Conflict
                    </Button>

                    <Button 
                        appearance="secondary"
                        onClick={() => testError(() => errorTestApi.testRateLimit(), "Rate Limit")}
                    >
                        Тест: 429 Rate Limit
                    </Button>
                </div>
            </Card>

            <Card>
                <Subtitle1>Критические ошибки (Toast Error)</Subtitle1>
                <div className="flex flex-wrap gap-[12px] mt-[16px]">
                    <Button 
                        appearance="secondary"
                        onClick={() => testError(() => errorTestApi.testNotFound(), "Not Found")}
                    >
                        Тест: 404 Not Found
                    </Button>

                    <Button 
                        appearance="secondary"
                        onClick={() => testError(() => errorTestApi.testCritical(), "Critical")}
                    >
                        Тест: 500 Critical Error
                    </Button>
                </div>
            </Card>

            <Card>
                <Subtitle1>Успешный запрос</Subtitle1>
                <div className="flex flex-wrap gap-[12px] mt-[16px]">
                    <Button 
                        appearance="primary"
                        onClick={async () => {
                            clearAllErrors();
                            setLastResult("Тестируем: Success...");
                            try {
                                const result = await errorTestApi.testSuccess();
                                showToast("Успешный запрос!", "success");
                                setLastResult(`Success: ${JSON.stringify(result.data)}`);
                            } catch (error) {
                                const result = handleError(error);
                                setLastResult(`Success Error: ${result.message}`);
                            }
                        }}
                    >
                        Тест: Успешный запрос
                    </Button>
                </div>
            </Card>

            <Card>
                <Subtitle1>Последний результат</Subtitle1>
                <Text className="mt-[8px] font-mono text-sm break-all">{lastResult || "Нажмите на любую кнопку для теста"}</Text>
                
                {Object.keys(fieldErrors).length > 0 && (
                    <div className="mt-[16px]">
                        <Text weight="bold">Ошибки полей:</Text>
                        <pre className="mt-[8px] p-[12px] bg-gray-100 rounded text-xs overflow-auto">
                            {JSON.stringify(fieldErrors, null, 2)}
                        </pre>
                    </div>
                )}
            </Card>

            <div className="flex gap-[12px]">
                <Button 
                    onClick={clearAllErrors}
                    appearance="outline"
                >
                    Очистить все ошибки
                </Button>
            </div>
        </div>
    );
};
