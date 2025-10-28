import {
    Title1,
    Subtitle1,
    Avatar,
    Body1,
    Tag,
    type AvatarNamedColor,
    Card,
    TabList,
    Tab,
} from "@fluentui/react-components";
import "./PersonalData.css";
import {type FC, useState} from "react";
import {useDispatch} from "react-redux";
import type {AppDispatch} from "../store";
import {updateClientProfile} from "../store/clientSlice";
import {ChangePasswordForm} from "../components/PersonalDataForm/ChangePasswordForm.tsx";
import {PersonalInfoForm} from "../components/PersonalDataForm/PersonalInfoForm.tsx";
import type {ClientInfo} from "../types/client";

export const PersonalData: FC = () => {
    const client: ClientInfo = {
        clientId: 1,
        firstName: "Даниил",
        lastName: "Иванов",
        middleName: "Иванович",
        email: "ivan@example.com",
        emailConfirmed: false,
        genderId: 1,
        birthDate: new Date(),
        phoneNumber: "+7 (999) 123-45-67",
        phoneNumberConfirmed: true,
        photo: "https://via.placeholder.com/200",
        accessCardNumber: "123456789",
        createdAt: "2023-01-01T12:00:00",
    };

    const getInitials = (client: ClientInfo): string => {
        const parts = [
            client.firstName ? client.firstName[0] : "",
            client.lastName ? client.lastName[0] : "",
        ].filter(Boolean);
        return parts.join("");
    };

    const getStatusText = (phoneNumberConfirmed?: boolean | null): string => {
        const statusMap: Record<string, string> = {
            "true": "Активный",
            "false": "Требует активности",
            "null": "Неактивный"
        };
        return statusMap[String(phoneNumberConfirmed)] || "Ошибка";
    };

    const getStatusClass = (confirmed?: boolean | null): string => {
        if (confirmed === true) return "dark-green";
        if (confirmed === false) return "pumpkin";
        if (confirmed == null) return "beige";
        return "dark-red";
    };

    const [activeTab, setActiveTab] = useState<'info' | 'password'>('info');
    const [clientState, setClientState] = useState<ClientInfo>(client);
    const dispatch = useDispatch<AppDispatch>();


    return (
        <div className="personal-data-root gap-[16px]">
            <Title1>Персональные данные</Title1>
            <div className="personal-data-section">
                <Card className="row flex-wrap">
                    <Avatar initials={getInitials(client).toString()}
                            color={getStatusClass(client.phoneNumberConfirmed) as AvatarNamedColor}
                            name="darkGreen avatar" size={128}
                            className=""/>
                    <div className="">
                        <Subtitle1 block truncate>
                            <strong>ФИО:</strong> {client.firstName} {client.lastName} {client.middleName || ""}
                        </Subtitle1>
                        <Body1 as="p" block>
                            <strong>Статус:</strong> <Tag className={getStatusClass(client.phoneNumberConfirmed)}
                                                          shape="circular"
                                                          appearance="outline"
                                                          size="extra-small">{getStatusText(client.phoneNumberConfirmed)}</Tag>

                        </Body1>
                        <Body1 as="p" block>
                            <strong>Дата
                                регистрации:</strong> {client.createdAt ? new Date(client.createdAt).toLocaleDateString() : "—"}
                        </Body1>
                    </div>
                </Card>


                <TabList selectedValue={activeTab}
                         appearance="subtle-circular"
                         onTabSelect={(_, data) => setActiveTab(data.value as 'info' | 'password')}>
                    <Tab value="info">Персональные данные</Tab>
                    <Tab value="password">Пароль</Tab>
                </TabList>

                {activeTab === 'info' && (
                    <PersonalInfoForm
                        className="flex-1"
                        client={clientState}
                        onChange={(changed) => setClientState(prev => ({...prev, ...changed}))}
                        onSave={(updated) => {
                            setClientState(prev => ({
                                ...prev,
                                ...updated,
                                createdAt: prev.createdAt
                            }));
                            dispatch(updateClientProfile({
                                email: updated.email,
                                phoneNumber: updated.phoneNumber,
                                birthDate: updated.birthDate,
                                genderId: updated.genderId,
                            }));
                        }}
                        showDownloadButton
                    />
                )}
                {activeTab === 'password' && (
                    <ChangePasswordForm
                        className="flex-1"
                        showCancelButton
                        onCancel={() => setActiveTab('info')}
                        redirectToLoginOnSuccess
                        autoClearTokensOnSuccess
                    />
                )}
            </div>
        </div>
    );
};