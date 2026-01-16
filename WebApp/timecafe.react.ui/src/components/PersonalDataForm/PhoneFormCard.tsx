import {createElement, useEffect, useState, type FC} from "react";
import {Button, Card, Tag, Text, Title2, Tooltip} from "@fluentui/react-components";
import {CheckmarkFilled, DismissFilled, Edit20Regular, type FluentIcon} from "@fluentui/react-icons";
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

            <div className="mt-3">
                <div className="flex flex-col gap-2">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                        <div className="flex items-center gap-2">
                            <Text weight="semibold">{phone || "—"}</Text>
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
                            icon={<Edit20Regular />}
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
