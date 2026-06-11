import { Body1, Body1Strong, Card, Divider } from "@fluentui/react-components";
import type { AuditLogDto } from "@store/api/adminApi";
import { formatDateTime } from "@utility/dateUtils";
import { formatDurationMs, safeParseJson } from "@utility/formatUtils";
import { NO_DATA } from "@shared/const/placeholders";
import type { FC } from "react";

interface AuditLogDetailsProps {
    log: AuditLogDto;
    userDisplayName?: React.ReactNode;
}

const isUuid = (str: string) =>
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(str);

export const AuditLogDetails: FC<AuditLogDetailsProps> = ({ log, userDisplayName }) => {
    const hasUserId = isUuid(log.userName);

    return (
        <div className="flex flex-col gap-4">
            <div className="flex flex-col gap-2 p-3.5 bg-(--colorNeutralBackground2) rounded-xl border border-(--colorNeutralStroke1)">
                <div className="flex flex-col gap-0.5">
                    <Body1 className="text-(--colorNeutralForeground3)">Исполнитель</Body1>
                    <Body1Strong className="text-base text-(--colorBrandForegroundLink)">{userDisplayName || log.userName}</Body1Strong>
                </div>
                {hasUserId && (
                    <div className="flex flex-col gap-0.5 mt-1 border-t border-(--colorNeutralStroke2) pt-1.5">
                        <Body1 className="text-(--colorNeutralForeground3)">ID пользователя (User ID)</Body1>
                        <span className="font-mono text-xs text-(--colorNeutralForeground2) break-all select-all">{log.userName}</span>
                    </div>
                )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="flex flex-col gap-0.5">
                    <Body1 className="text-(--colorNeutralForeground3)">ID записи аудита</Body1>
                    <Body1Strong>{log.id}</Body1Strong>
                </div>
                <div className="flex flex-col gap-0.5">
                    <Body1 className="text-(--colorNeutralForeground3)">Дата</Body1>
                    <Body1Strong>{formatDateTime(log.createdAt)}</Body1Strong>
                </div>
                <div className="flex flex-col gap-0.5">
                    <Body1 className="text-(--colorNeutralForeground3)">Тип события</Body1>
                    <Body1Strong>{log.eventType}</Body1Strong>
                </div>
                <div className="flex flex-col gap-0.5">
                    <Body1 className="text-(--colorNeutralForeground3)">Действие</Body1>
                    <Body1Strong>{log.action}</Body1Strong>
                </div>
                <div className="flex flex-col gap-0.5">
                    <Body1 className="text-(--colorNeutralForeground3)">Машина/Домен</Body1>
                    <Body1Strong>{log.machineName} ({log.domainName})</Body1Strong>
                </div>
                <div className="flex flex-col gap-0.5">
                    <Body1 className="text-(--colorNeutralForeground3)">Длительность</Body1>
                    <Body1Strong>{formatDurationMs(log.duration)}</Body1Strong>
                </div>
                <div className="flex flex-col gap-0.5">
                    <Body1 className="text-(--colorNeutralForeground3)">Correlation ID</Body1>
                    <Body1Strong>{log.correlationId || NO_DATA}</Body1Strong>
                </div>
            </div>
            <Divider />
            <div className="flex flex-col gap-2">
                <Body1Strong>Полные JSON данные лога</Body1Strong>
                <Card appearance="filled-alternative" className="max-h-[400px] overflow-y-auto">
                    <pre className="font-mono text-xs whitespace-pre-wrap overflow-x-auto">
                        {JSON.stringify(
                            {
                                ...log,
                                oldData: safeParseJson(log.oldData),
                                newData: safeParseJson(log.newData),
                                environmentJson: safeParseJson(log.environmentJson),
                                customFieldsJson: safeParseJson(log.customFieldsJson),
                                comments: safeParseJson(log.comments),
                            },
                            null,
                            2
                        )}
                    </pre>
                </Card>
            </div>
        </div>
    );
};
