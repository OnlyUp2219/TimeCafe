import React, { useState, useCallback } from "react";
import {
    Title2,
    Title3,
    Button,
    Card,
    Switch,
    Body1,
    Caption1,
    MessageBar,
    MessageBarBody,
    MessageBarActions,
    Divider,
    Badge,
    Body1Strong,
} from "@fluentui/react-components";
import {
    Bug24Regular,
    CheckmarkCircle24Regular,
    Info24Regular,
    Warning24Regular,
    ArrowSync24Regular,
    Document24Regular,
    DismissRegular
} from "@fluentui/react-icons";
import { useProgressToast } from "@components/ToastProgress/ToastProgress";
import {
    useLazyGetError404SingleQuery,
    useLazyGetError404MultipleQuery,
    useLazyGetError500SingleQuery,
    useLazyGetError500MultipleQuery,
    useLazyGetErrorExceptionQuery,
    useLazyGetSuccessQuery,
    useLazyGetInfoQuery,
    useLazyGetLegacyResultQuery
} from "@store/api/debugApi";
import { extractRtkError, getRtkErrorMessage, getRtkErrorTitle, normalizeUnknownError } from "@shared/api/errors/extractRtkError";

export const DevDebugPage: React.FC = () => {
    const { showToast, ToasterElement } = useProgressToast();
    const [useToast, setUseToast] = useState(true);
    const [localError, setLocalError] = useState<string | null>(null);
    const [lastResponse, setLastResponse] = useState<any>(null);
    const [extractedInfo, setExtractedInfo] = useState<any>(null);
    const [currentIntent, setCurrentIntent] = useState<"success" | "info" | "warning" | "error">("error");

    const [trigger404Single] = useLazyGetError404SingleQuery();
    const [trigger422Multiple] = useLazyGetError404MultipleQuery();
    const [trigger500Single] = useLazyGetError500SingleQuery();
    const [trigger500Multiple] = useLazyGetError500MultipleQuery();
    const [triggerException] = useLazyGetErrorExceptionQuery();
    const [triggerSuccess] = useLazyGetSuccessQuery();
    const [triggerInfo] = useLazyGetInfoQuery();
    const [triggerLegacy] = useLazyGetLegacyResultQuery();

    const handleAction = useCallback(async (action: () => Promise<any>, name: string) => {
        setLocalError(null);
        setLastResponse(null);
        setExtractedInfo(null);

        try {
            const result = await action();

            if (result.error) {
                const errorData = result.error;
                const extracted = extractRtkError(errorData);
                if (!extracted) return;

                const message = extracted.message;
                const isValidation = extracted.statusCode === 422;
                const intent = isValidation ? "warning" : "error";

                const title = getRtkErrorTitle(errorData, name);

                setLastResponse(errorData);
                setExtractedInfo(extracted);
                setCurrentIntent(intent);

                if (useToast) {
                    showToast(message, intent, title);
                } else {
                    setLocalError(message);
                }
            } else {
                const data = result.data;
                const extracted = normalizeUnknownError(data);

                setLastResponse(data);
                setExtractedInfo(extracted);

                const message = data?.message || "Операция выполнена";
                const isInfo = data?.type === "Info" || data?.code?.toLowerCase().includes("info");
                const intent = isInfo ? "info" : "success";
                const title = extracted.code || (isInfo ? "Info" : "Success");

                setCurrentIntent(intent);

                if (useToast) {
                    showToast(message, intent, title);
                } else {
                    setLocalError(message);
                }
            }
        } catch (e: any) {
            const msg = e.message || "Неизвестная ошибка UI";
            setCurrentIntent("error");
            if (useToast) showToast(msg, "error", "UI Crash");
            else setLocalError(msg);
        }
    }, [useToast, showToast]);

    const renderBlock = (title: string, icon: React.ReactNode, children: React.ReactNode) => (
        <Card className="p-4 flex flex-col gap-3">
            <div className="flex items-center gap-2 mb-2">
                {icon}
                <Title3>{title}</Title3>
            </div>
            <div className="flex flex-wrap gap-2">
                {children}
            </div>
        </Card>
    );

    return (
        <div className="flex flex-col gap-6 p-4 max-w-5xl">
            {ToasterElement}

            <div className="flex items-center justify-between">
                <Title2>Debug & Error Handling</Title2>
                <div className="flex items-center gap-4 bg-gray-100 p-2 rounded-lg">
                    <Body1>Режим уведомлений:</Body1>
                    <Switch
                        label={useToast ? "Toast (Всплывающее)" : "MessageBar (Локальное)"}
                        checked={useToast}
                        onChange={(_, data) => setUseToast(data.checked)}
                    />
                </div>
            </div>

            {localError && (
                <MessageBar intent={currentIntent}>
                    <MessageBarBody>
                        <div className="flex flex-col">
                            <Body1 italic className="opacity-70 mb-1">
                                {currentIntent.toUpperCase()} Message:
                            </Body1>
                            <Body1Strong>{localError}</Body1Strong>
                        </div>
                    </MessageBarBody>
                    <MessageBarActions
                        containerAction={
                            <Button
                                aria-label="dismiss"
                                appearance="subtle"
                                icon={<DismissRegular />}
                                onClick={() => setLocalError(null)}
                            />
                        }
                    />
                </MessageBar>
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {renderBlock("404 & 422 Errors", <Warning24Regular className="text-orange-500" />, (
                    <>
                        <Button appearance="outline" onClick={() => handleAction(trigger404Single, "404 Single")}>
                            404 Single (Not Found)
                        </Button>
                        <Button appearance="outline" onClick={() => handleAction(trigger422Multiple, "422 Multiple")}>
                            422 Multiple (Validation)
                        </Button>
                    </>
                ))}

                {renderBlock("500 & Exceptions", <Bug24Regular className="text-red-600" />, (
                    <>
                        <Button appearance="outline" onClick={() => handleAction(trigger500Single, "500 Single")}>
                            500 Single Error
                        </Button>
                        <Button appearance="outline" onClick={() => handleAction(trigger500Multiple, "500 Multiple")}>
                            500 Multiple Technical
                        </Button>
                        <Button appearance="primary" icon={<Bug24Regular />} onClick={() => handleAction(triggerException, "Unhandled Exception")}>
                            Throw Exception
                        </Button>
                    </>
                ))}

                {renderBlock("Success & Info", <CheckmarkCircle24Regular className="text-green-600" />, (
                    <>
                        <Button appearance="outline" onClick={() => handleAction(triggerSuccess, "Success Response")}>
                            Success (200 OK)
                        </Button>
                        <Button appearance="outline" onClick={() => handleAction(triggerInfo, "Info Response")}>
                            Info Message
                        </Button>
                    </>
                ))}

                {renderBlock("Legacy & RTK", <ArrowSync24Regular className="text-blue-500" />, (
                    <>
                        <Button appearance="outline" onClick={() => handleAction(triggerLegacy, "Legacy Format")}>
                            Legacy CqrsResult
                        </Button>
                        <Button appearance="subtle" onClick={() => {
                            if (useToast) showToast("Это тестовый тост без запроса", "info", "Local UI");
                            else setLocalError("Это локальный MessageBar без запроса");
                        }}>
                            Local UI Toggle
                        </Button>
                    </>
                ))}
            </div>

            <Divider />

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Card className="bg-black text-green-400 p-4 font-mono text-xs overflow-auto max-h-96">
                    <div className="flex items-center gap-2 mb-2 text-white border-b border-gray-700 pb-2">
                        <Document24Regular />
                        <Body1>RAW API RESPONSE</Body1>
                    </div>
                    <pre>{lastResponse ? JSON.stringify(lastResponse, null, 2) : "Ожидание запроса..."}</pre>
                </Card>

                <Card className="p-4 bg-gray-50 overflow-auto max-h-96">
                    <div className="flex items-center gap-2 mb-2 border-b pb-2">
                        <Info24Regular />
                        <Body1>EXTRACTED INFO (extractRtkError)</Body1>
                    </div>
                    {extractedInfo ? (
                        <div className="flex flex-col gap-2">
                            <div className="flex justify-between">
                                <Caption1>Status Code:</Caption1>
                                <Body1>{extractedInfo.statusCode}</Body1>
                            </div>
                            <div className="flex justify-between">
                                <Caption1>Main Message:</Caption1>
                                <Body1 className="text-blue-700 font-bold">{extractedInfo.message}</Body1>
                            </div>
                            <Divider />
                            <Caption1>Detailed Errors ({extractedInfo.errors?.length || 0}):</Caption1>
                            <div className="flex flex-col gap-2 mt-1">
                                {extractedInfo.errors?.length ? extractedInfo.errors.map((e: any, i: number) => (
                                    <div key={i} className="flex flex-col p-2 bg-white rounded border border-gray-200 shadow-sm">
                                        <div className="flex items-center gap-2 mb-1">
                                            {e.code && <Badge appearance="filled" color="brand" size="small">{e.code}</Badge>}
                                            <Body1Strong>Field/Code</Body1Strong>                                        </div>
                                        <Body1 className="text-red-600">{e.message}</Body1>
                                    </div>
                                )) : (
                                    <Body1 italic className="text-gray-400">Нет детальных ошибок (только общее сообщение)</Body1>
                                )}
                            </div>
                            <Divider />
                            <div className="mt-2">
                                <Caption1>Final UI Message (getRtkErrorMessage):</Caption1>
                                <div className="bg-yellow-100 p-2 mt-1 rounded border border-yellow-200">
                                    <Body1>{getRtkErrorMessage(lastResponse)}</Body1>
                                </div>
                            </div>
                        </div>
                    ) : (
                        "Нажми кнопку для анализа ошибки..."
                    )}
                </Card>
            </div>
        </div>
    );
};
