import {createElement, useEffect, useState, type FC} from "react";
import {Body1Strong, Body2, Button, Card, Tag, Title2, Tooltip} from "@fluentui/react-components";
import {CheckmarkFilled, DismissFilled, Edit20Filled, PhoneRegular, type FluentIcon} from "@fluentui/react-icons";
import type {ClientInfo} from "../../types/client.ts";
import {PhoneVerificationModal} from "../PhoneVerificationModal/PhoneVerificationModal.tsx";

type PhoneVerificationSessionV1 = {
    open: boolean;
    step: "input" | "verify";
    phoneNumber: string;
    mode: "api" | "ui";
    uiGeneratedCode?: string | null;
};

const PHONE_VERIFICATION_SESSION_KEY = "tc_phone_verification_session_v1";

const loadPhoneSession = (): PhoneVerificationSessionV1 | null => {
    try {
        const raw = window.localStorage.getItem(PHONE_VERIFICATION_SESSION_KEY);
        if (!raw) return null;
        const parsed = JSON.parse(raw) as PhoneVerificationSessionV1;
        if (!parsed || typeof parsed !== "object") return null;
        if (parsed.step !== "input" && parsed.step !== "verify") return null;
        if (parsed.mode !== "api" && parsed.mode !== "ui") return null;
        return {
            open: Boolean(parsed.open),
            step: parsed.step,
            phoneNumber: String(parsed.phoneNumber ?? ""),
            mode: parsed.mode,
            uiGeneratedCode: parsed.uiGeneratedCode ?? null,
        };
    } catch {
        return null;
    }
};

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
    const [showPhoneModal, setShowPhoneModal] = useState(false);

    const phone = client.phoneNumber || "";

    useEffect(() => {
        const session = loadPhoneSession();
        if (session?.open && session.step === "verify") {
            setShowPhoneModal(true);
        }
    }, []);

    const handlePhoneVerified = (verifiedPhone: string) => {
        onSave?.({phoneNumber: verifiedPhone, phoneNumberConfirmed: true});
    };

    return (
        <Card className={className}>
            <Title2 block className="!flex gap-2">
                <div className="flex items-center gap-2 w-10 h-10 justify-center brand-badge rounded-full">
                    <PhoneRegular />
                </div>
                Телефон
            </Title2>
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
                currentPhoneNumber={client.phoneNumber || ""}
                currentPhoneNumberConfirmed={client.phoneNumberConfirmed === true}
                onSuccess={handlePhoneVerified}
                mode="ui"
            />
        </Card>
    );
};
