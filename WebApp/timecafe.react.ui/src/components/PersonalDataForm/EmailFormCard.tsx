import {createElement, useEffect, useState, type FC} from "react";
import {Body1Strong, Body2, Button, Card, Tag, Title2, Tooltip} from "@fluentui/react-components";
import {CheckmarkFilled, DismissFilled, Edit20Filled, MailRegular, type FluentIcon} from "@fluentui/react-icons";
import type {ClientInfo} from "../../types/client.ts";
import {EmailVerificationModal} from "../EmailVerificationModal/EmailVerificationModal";

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
    const [showEmailModal, setShowEmailModal] = useState(false);

    useEffect(() => {
        setEmail(client.email);
    }, [client.clientId, client.email]);

    const handleEmailVerified = (verifiedEmail: string) => {
        setEmail(verifiedEmail);
        onSave?.({email: verifiedEmail, emailConfirmed: true});
    };

    return (
        <Card className={className}>
            <Title2 block className="!flex gap-2">
                <div className="flex items-center gap-2 w-10 h-10 justify-center brand-badge rounded-full">
                    <MailRegular />
                </div>
                Почта
            </Title2>
            <Body2 className="!line-clamp-2">
                Нужна для важных уведомлений и восстановления доступа.
            </Body2>

            <div className="flex flex-col gap-3">
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
                            onClick={() => setShowEmailModal(true)}
                            disabled={loading}
                        >
                            {client.emailConfirmed ? "Изменить" : "Подтвердить"}
                        </Button>
                    </div>
                </div>
            </div>

            <EmailVerificationModal
                isOpen={showEmailModal}
                onClose={() => setShowEmailModal(false)}
                currentEmail={email}
                currentEmailConfirmed={client.emailConfirmed === true}
                onSuccess={handleEmailVerified}
            />
        </Card>
    );
};
