import {createElement, useEffect, useState, type FC} from "react";
import {Body1Strong, Body2, Button, Card, Tag, Title2, Tooltip} from "@fluentui/react-components";
import {CheckmarkFilled, DismissFilled, Edit20Filled, type FluentIcon} from "@fluentui/react-icons";
import type {ClientInfo} from "../../types/client.ts";
import {EmailInput} from "../FormFields";

export interface EmailFormCardProps {
    client: ClientInfo;
    loading?: boolean;
    className?: string;
    onSave?: (patch: Partial<ClientInfo>) => void;
}

const getStatusClass = (confirmed?: boolean | null): string => {
    if (confirmed === true) return "dark-green";
    if (confirmed === false) return "pumpkin";
    if (confirmed == null) return "beige";
    return "dark-red";
};

const getStatusIcon = (confirmed?: boolean | null): FluentIcon => {
    if (confirmed) return CheckmarkFilled;
    return DismissFilled;
};

export const EmailFormCard: FC<EmailFormCardProps> = ({client, loading = false, className, onSave}) => {
    const [email, setEmail] = useState(client.email);
    const [mode, setMode] = useState<"view" | "edit">("view");

    useEffect(() => {
        setEmail(client.email);
    }, [client.clientId, client.email]);

    const handleSave = () => {
        onSave?.({email});
        setMode("view");
    };

    const handleCancel = () => {
        setEmail(client.email);
        setMode("view");
    };

    const isEditing = mode === "edit";

    return (
        <Card className={className}>
            <Title2>Почта</Title2>
            <Body2 className="!line-clamp-2">
                Нужна для важных уведомлений и восстановления доступа.
            </Body2>

            <div className="flex flex-col gap-3">
                {!isEditing ? (
                    <div className="flex flex-col gap-2">
                        <div className="flex flex-col sm:items-center sm:flex-row justify-between gap-2 min-w-0">
                            <div className="flex items-center gap-2 min-w-0">
                                <Tooltip content={`Почта: ${email || "не указан"}`} relationship="label">
                                    <Body1Strong className="!line-clamp-1 max-w-[25ch] md:max-w-[40ch] !truncate">
                                        {email || "—"}
                                    </Body1Strong>
                                </Tooltip>
                                <Tooltip
                                    content={client.emailConfirmed ? "Email подтверждён" : "Email не подтверждён"}
                                    relationship="description"
                                >
                                    <Tag
                                        appearance="outline"
                                        icon={createElement(getStatusIcon(client.emailConfirmed))}
                                        className={`custom-tag ${getStatusClass(client.emailConfirmed)}`}
                                    />
                                </Tooltip>
                            </div>
                            <Button
                                appearance="primary"
                                icon={<Edit20Filled/>}
                                onClick={() => setMode("edit")}
                                disabled={loading}
                            >
                                Изменить
                            </Button>
                        </div>

                    </div>
                ) : (
                    <>
                        <EmailInput
                            value={email}
                            onChange={(value) => {
                                setEmail(value);
                            }}
                            disabled={loading}
                            shouldValidate
                            trailingElement={
                                <Tag
                                    appearance="outline"
                                    icon={createElement(getStatusIcon(client.emailConfirmed))}
                                    className={`custom-tag ${getStatusClass(client.emailConfirmed)}`}
                                />
                            }
                        />

                        <div className="flex flex-nowrap items-center justify-end gap-2">
                            <Button appearance="primary" onClick={handleSave} disabled={loading || !email.trim()}>
                                Сохранить
                            </Button>
                            <Button appearance="secondary" onClick={handleCancel} disabled={loading}>
                                Отмена
                            </Button>
                        </div>
                    </>
                )}
            </div>
        </Card>
    );
};
