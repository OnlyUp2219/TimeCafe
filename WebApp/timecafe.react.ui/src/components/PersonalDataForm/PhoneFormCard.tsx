import {createElement, useEffect, useState, type FC} from "react";
import {Badge, Body1Strong, Body2, Button, Card, Tag, Title2, Tooltip} from "@fluentui/react-components";
import {CheckmarkFilled, DismissFilled, Edit20Filled, PhoneRegular, type FluentIcon} from "@fluentui/react-icons";
import type {Profile} from "../../types/profile";
import {PhoneVerificationModal} from "../PhoneVerificationModal/PhoneVerificationModal.tsx";
import {
    isPhoneVerificationSessionV1,
    PHONE_VERIFICATION_SESSION_KEY,
    type PhoneVerificationSessionV1,
} from "../../shared/auth/phoneVerificationSession";
import {useLocalStorageJson} from "../../hooks/useLocalStorageJson";

export interface PhoneFormCardProps {
    profile: Profile;
    loading?: boolean;
    className?: string;
    onSave?: (patch: Partial<Profile>) => void;
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

export const PhoneFormCard: FC<PhoneFormCardProps> = ({profile, loading = false, className, onSave}) => {
    const phoneSessionStore = useLocalStorageJson<PhoneVerificationSessionV1>(
        PHONE_VERIFICATION_SESSION_KEY,
        isPhoneVerificationSessionV1
    );

    const [showPhoneModal, setShowPhoneModal] = useState(false);
    const [phoneModalMode, setPhoneModalMode] = useState<"api" | "ui">("ui");

    const phone = profile.phoneNumber || "";
    const hasPhone = Boolean(phone.trim());
    const confirmedForUi: boolean | null = hasPhone ? (profile.phoneNumberConfirmed ?? null) : null;
    const actionLabel = !hasPhone ? "Заполнить" : profile.phoneNumberConfirmed ? "Изменить" : "Подтвердить";

    useEffect(() => {
        const session = phoneSessionStore.load();
        if (session?.open && session.step === "verify") {
            setPhoneModalMode(session.mode);
            setShowPhoneModal(true);
        }
    }, []);

    const handlePhoneVerified = (verifiedPhone: string) => {
        onSave?.({phoneNumber: verifiedPhone, phoneNumberConfirmed: true});
    };

    return (
        <Card className={className}>
            <Title2 block className="!flex items-center gap-2">
                <Badge appearance="tint" shape="rounded" size="extra-large" className="brand-badge">
                    <PhoneRegular className="size-5" />
                </Badge>
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
                                content={!hasPhone ? "Телефон не указан" : (profile.phoneNumberConfirmed ? "Телефон подтверждён" : "Телефон не подтверждён")}
                                relationship="description"
                            >
                                <Tag
                                    appearance="outline"
                                    icon={createElement(getStatusIcon(confirmedForUi))}
                                    className={`custom-tag ${getStatusClass(confirmedForUi)}`}
                                />
                            </Tooltip>
                        </div>

                        <Button
                            appearance="primary"
                            icon={<Edit20Filled/>}
                            onClick={() => setShowPhoneModal(true)}
                            disabled={loading}
                        >
                            {actionLabel}
                        </Button>
                    </div>
                </div>
            </div>

            <PhoneVerificationModal
                isOpen={showPhoneModal}
                onClose={() => setShowPhoneModal(false)}
                currentPhoneNumber={profile.phoneNumber || ""}
                currentPhoneNumberConfirmed={profile.phoneNumberConfirmed === true}
                onSuccess={handlePhoneVerified}
                mode={phoneModalMode}
            />
        </Card>
    );
};
