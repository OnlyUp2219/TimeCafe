import {createElement, useEffect, useState, type FC} from "react";
import {Body1Strong, Body2, Button, Card, Tag, Title2, Tooltip} from "@fluentui/react-components";
import {CheckmarkFilled, DismissFilled, Edit20Filled, type FluentIcon} from "@fluentui/react-icons";
import type {ClientInfo} from "../../types/client.ts";
import {PhoneVerificationModal} from "../PhoneVerificationModal/PhoneVerificationModal.tsx";

export interface PhoneFormCardProps {
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

export const PhoneFormCard: FC<PhoneFormCardProps> = ({client, loading = false, className, onSave}) => {
    const [phone, setPhone] = useState(client.phoneNumber || "");
    const [showPhoneModal, setShowPhoneModal] = useState(false);

    useEffect(() => {
        setPhone(client.phoneNumber || "");
    }, [client.clientId, client.phoneNumber]);

    const handlePhoneVerified = (verifiedPhone: string) => {
        setPhone(verifiedPhone);
        onSave?.({phoneNumber: verifiedPhone, phoneNumberConfirmed: true});
    };

    return (
        <Card className={className}>
            <Title2>Телефон</Title2>
            <Body2 className="!line-clamp-2">
                Используется для уведомлений и подтверждения номера.
            </Body2>

            <div>
                <div className="flex flex-col gap-2">
                    <div className="flex flex-col sm:items-center sm:flex-row justify-between gap-2 min-w-0">
                        <div className="flex items-center gap-2 min-w-0 ">
                            <Tooltip content={`Телефон: ${phone || "не указан"}`} relationship="label">
                                <Body1Strong className="!line-clamp-1 max-w-[25ch] md:max-w-[40ch] !truncate">
                                    {phone || "—"}
                                </Body1Strong>
                            </Tooltip>
                            <Tooltip
                                content={client.phoneNumberConfirmed ? "Телефон подтверждён" : "Телефон не подтверждён"}
                                relationship="description"
                            >
                                <Tag
                                    appearance="outline"
                                    icon={createElement(getStatusIcon(client.phoneNumberConfirmed))}
                                    className={`custom-tag ${getStatusClass(client.phoneNumberConfirmed)}`}
                                />
                            </Tooltip>
                        </div>

                        <Button
                            appearance="primary"
                            icon={<Edit20Filled/>}
                            onClick={() => setShowPhoneModal(true)}
                            disabled={loading}
                        >
                            {client.phoneNumberConfirmed ? "Изменить" : "Подтвердить"}
                        </Button>
                    </div>
                </div>
            </div>

            <PhoneVerificationModal
                isOpen={showPhoneModal}
                onClose={() => setShowPhoneModal(false)}
                currentPhoneNumber={phone}
                onSuccess={handlePhoneVerified}
            />
        </Card>
    );
};
