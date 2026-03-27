import type {FC} from "react";
import {Body2, Button, Caption1, Divider, Subtitle2Stronger, Tag, Text, Title3} from "@fluentui/react-components";
import {PersonRegular} from "@fluentui/react-icons";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import type {Profile} from "@app-types/profile";

interface ProfileCardProps {
    profile: Profile | undefined;
    email: string;
    phone: string;
    onNavigate: () => void;
}

export const ProfileCard: FC<ProfileCardProps> = ({profile, email, phone, onNavigate}) => (
    <HoverTiltCard className="flex flex-col justify-between lg:col-span-7">
        <div className="flex flex-col gap-4">
            <div className="flex items-center gap-2 flex-wrap justify-between">
                <div className="flex items-center gap-2">
                    <PersonRegular/>
                    <Subtitle2Stronger>Профиль</Subtitle2Stronger>
                </div>
                <Tag appearance={profile ? "brand" : "outline"}>
                    {profile ? "Заполнен" : "Не заполнен"}
                </Tag>
            </div>
            <Divider className="divider grow-0"/>
        </div>

        {profile ? (
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <div className="flex flex-col gap-1 min-w-0">
                    <Caption1>ФИО</Caption1>
                    <Title3>{`${profile.lastName} ${profile.firstName}${profile.middleName ? ` ${profile.middleName}` : ""}`}</Title3>
                </div>
                <div className="flex flex-col gap-1 sm:text-right min-w-0">
                    <Caption1>Контакты</Caption1>
                    <Body2>{email}</Body2>
                    <Caption1>{phone}</Caption1>
                </div>
            </div>
        ) : (
            <div className="flex flex-col gap-2">
                <Body2>
                    Заполните профиль, чтобы продолжить пользовательский сценарий.
                </Body2>
                <div className="flex flex-col gap-2 sm:flex-row sm:flex-nowrap sm:min-w-0">
                    <Button appearance="primary" onClick={onNavigate}>
                        <Text truncate wrap={false}>
                            Заполнить профиль
                        </Text>
                    </Button>
                </div>
            </div>
        )}
    </HoverTiltCard>
);
