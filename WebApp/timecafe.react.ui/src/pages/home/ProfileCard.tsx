import type { FC } from "react";
import { Body2, Button, Divider, Subtitle2Stronger, Tag, Text, Title3, Caption1 } from "@fluentui/react-components";
import { PersonRegular, Mail20Regular, Phone20Regular, Warning20Regular, Checkmark20Regular, Dismiss20Regular } from "@fluentui/react-icons";
import { HoverTiltCard } from "@components/HoverTiltCard/HoverTiltCard";
import type { Profile } from "@app-types/profile";
import { useComponentSize } from "@hooks/useComponentSize";
import { SecureAvatar } from "@components/SecureAvatar/SecureAvatar";

interface ProfileCardProps {
    profile: Profile | undefined;
    email: string;
    phone: string;
    emailConfirmed?: boolean;
    phoneConfirmed?: boolean;
    onNavigate: () => void;
}

export const ProfileCard: FC<ProfileCardProps> = ({ profile, email, phone, emailConfirmed, phoneConfirmed, onNavigate }) => {
    const { sizes } = useComponentSize();

    const renderContent = () => {
        if (profile) {
            const displayName = `${profile.firstName} ${profile.lastName}`;
            return (
                <div className="flex flex-col sm:flex-row gap-4 items-center sm:items-start mt-4">
                    <SecureAvatar
                        size={72}
                        name={displayName}
                        photoUrl={profile.photoUrl}
                        className="shrink-0"
                    />
                    <div className="flex flex-col gap-2 min-w-0 flex-1 w-full">
                        <Title3 className="font-semibold text-center sm:text-left truncate">
                            {`${profile.firstName}${profile.middleName ? ` ${profile.middleName}` : ""}${profile.lastName ? ` ${profile.lastName}` : ""}`}
                        </Title3>
                        <Divider className="my-1" />
                        <div className="flex flex-col gap-4 mt-1.5 w-full">
                            <div className="flex items-start gap-2.5 min-w-0">
                                <Mail20Regular style={{ fontSize: "18px" }} className="text-(--colorBrandForeground1) shrink-0 mt-0.5" />
                                <div className="flex flex-col min-w-0 gap-1">
                                    <div className="flex flex-col">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Email</Caption1>
                                        <Text className="truncate font-medium">{email || "Не указана"}</Text>
                                    </div>
                                    {email && (
                                        <div className="flex items-center gap-1">
                                            {emailConfirmed ? (
                                                <>
                                                    <Checkmark20Regular className="text-(--colorStatusSuccessForeground1)" />
                                                    <Caption1 className="text-(--colorStatusSuccessForeground1)">Подтверждён</Caption1>
                                                </>
                                            ) : (
                                                <>
                                                    <Dismiss20Regular className="text-(--colorStatusDangerForeground1)" />
                                                    <Caption1 className="text-(--colorStatusDangerForeground1)">Не подтверждён</Caption1>
                                                </>
                                            )}
                                        </div>
                                    )}
                                </div>
                            </div>

                            <div className="flex items-start gap-2.5 min-w-0">
                                <Phone20Regular style={{ fontSize: "18px" }} className="text-(--colorBrandForeground1) shrink-0 mt-0.5" />
                                <div className="flex flex-col min-w-0 gap-1">
                                    <div className="flex flex-col">
                                        <Caption1 className="text-(--colorNeutralForeground3)">Телефон</Caption1>
                                        <Text className="font-medium">{phone || "Не указан"}</Text>
                                    </div>
                                    {phone && (
                                        <div className="flex items-center gap-1">
                                            {phoneConfirmed ? (
                                                <>
                                                    <Checkmark20Regular className="text-(--colorStatusSuccessForeground1)" />
                                                    <Caption1 className="text-(--colorStatusSuccessForeground1)">Подтверждён</Caption1>
                                                </>
                                            ) : (
                                                <>
                                                    <Dismiss20Regular className="text-(--colorStatusDangerForeground1)" />
                                                    <Caption1 className="text-(--colorStatusDangerForeground1)">Не подтверждён</Caption1>
                                                </>
                                            )}
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            );
        } else {
            return (
                <div className="flex flex-col gap-3 items-center text-center p-4 h-full justify-center">
                    <Warning20Regular className="text-(--colorStatusWarningForeground1)" style={{ fontSize: "40px", height: "40px", width: "40px" }} />
                    <Body2 className="max-w-[400px]">
                        Заполните профиль, чтобы продолжить пользовательский сценарий.
                    </Body2>
                    <div className="flex flex-col gap-2 sm:flex-row sm:flex-nowrap sm:min-w-0 mt-2">
                        <Button appearance="primary" size={sizes.button} onClick={onNavigate}>
                            <Text truncate wrap={false}>
                                Заполнить профиль
                            </Text>
                        </Button>
                    </div>
                </div>
            );
        }
    };

    return (
        <HoverTiltCard className="flex flex-col justify-start gap-2 lg:col-span-7 min-h-[320px]" size={sizes.card}>
            <div className="flex flex-col gap-4">
                <div className="flex items-center gap-2 flex-wrap">
                    <div className="flex items-center gap-2">
                        <PersonRegular />
                        <Subtitle2Stronger>Профиль</Subtitle2Stronger>
                    </div>
                    <Tag appearance={profile ? "brand" : "outline"}>
                        {profile ? "Заполнен" : "Не заполнен"}
                    </Tag>
                </div>
                <Divider className="divider grow-0" />
            </div>

            {renderContent()}

        </HoverTiltCard>
    );
};
