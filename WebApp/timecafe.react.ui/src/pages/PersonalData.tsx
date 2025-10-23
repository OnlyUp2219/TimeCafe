import {
    Button,
    Title2,
    Subtitle1,
    Avatar,
    Body1,
    Body2,
    Tag,
    Field,
    Input,
    Radio,
    RadioGroup, type AvatarNamedColor,
    Card,
} from "@fluentui/react-components";
import "./PersonalData.css";
import {createElement, type FC, useState} from "react";
import {CheckmarkFilled, DismissFilled, type FluentIcon} from '@fluentui/react-icons';

interface Client {
    clientId: number;
    firstName: string;
    lastName: string;
    middleName?: string;
    email: string;
    emailConfirmed?: boolean;
    genderId?: number;
    birthDate?: Date;
    phoneNumber?: string;
    phoneNumberConfirmed?: boolean | null;
    accessCardNumber?: string;
    photo?: string;
    createdAt: string;
}

export const PersonalData: FC = () => {
    const client: Client = {
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

    const getInitials = (client: Client): string => {
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

    const getStatusIcon = (Confirmed?: boolean | null): FluentIcon => {
        if (Confirmed) return CheckmarkFilled;
        else return DismissFilled;
    }


    const [email, setEmail] = useState(client.email);
    const [phone, setPhone] = useState(client.phoneNumber);
    const [date, setDate] = useState(client.birthDate);
    const handleSubmit = () => {
        console.log("Email submitted:", email);
    };


    return (
        <div className="personal-data-root gap-[22px]">
            <Title2>Персональные данные</Title2>
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
                            <strong>Дата регистрации:</strong> {new Date(client.createdAt).toLocaleDateString()}
                        </Body1>
                    </div>
                </Card>

                <Card>
                    <div className="flex w-full gap-[12px]
                    justify-stretch flex-wrap flex-row">
                        <div className="flex-1">
                            <Field label="Электронная почта" className="w-full">
                                <div className="input-with-button">
                                    <Input
                                        value={email}
                                        onChange={(e) => setEmail(e.target.value)}
                                        placeholder="Введите почту"
                                        className="w-full"
                                        type="email"
                                    />
                                    <Tag appearance="outline" icon={createElement(getStatusIcon(client.emailConfirmed))}
                                         className={`custom-tag ${getStatusClass(client.emailConfirmed)}`}/>
                                </div>
                            </Field>

                            <Field label="Телефон">
                                <div className="input-with-button">
                                    <Input
                                        value={phone}
                                        onChange={(e) => setPhone(e.target.value)}
                                        placeholder="Введите номер телефона"
                                        className="w-full"
                                        type="tel"

                                    />
                                    <Tag appearance="outline"
                                         icon={createElement(getStatusIcon(client.phoneNumberConfirmed))}
                                         className={`custom-tag ${getStatusClass(client.phoneNumberConfirmed)}`}/>
                                </div>
                            </Field>
                        </div>

                        <div className="flex-1">
                            <Field label="Дата рождения">
                                <Input
                                    value={date ? date.toISOString().split("T")[0] : ""}
                                    onChange={(e) => setDate(e.target.value ? new Date(e.target.value) : undefined)}
                                    placeholder="Введите номер телефона"
                                    type="date"
                                    className="w-full"
                                />
                            </Field>

                            <Field label="Пол">
                                <RadioGroup>
                                    <Radio value="Мужчина" label="Мужчина"/>
                                    <Radio value="Женщина" label="Женщина"/>
                                    <Radio value="Другое" label="Другое"/>
                                </RadioGroup>
                            </Field>
                        </div>
                    </div>
                </Card>


                <Body2><strong>Номер карты доступа:</strong> {client.accessCardNumber}</Body2>

                <Button appearance="primary">
                    Скачать данные
                </Button>
            </div>


        </div>
    );
};