import * as React from "react";
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
    RadioGroup,
} from "@fluentui/react-components";
import "./PersonalData.css";
import { useState } from "react";
import { CheckmarkFilled } from '@fluentui/react-icons';

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

export const PersonalData: React.FC = () => {
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


    const [email, setEmail] = useState(client.email);
    const [phone, setPhone] = useState(client.phoneNumber);
    const [date, setDate] = useState(client.birthDate);
    const handleSubmit = () => {
        console.log("Email submitted:", email);
    };


    return (
        <div className="personal-data-root">
            <Title2>Персональные данные</Title2>
            <div className="personal-data-section">

                <div className="row">
                    <Avatar initials={getInitials(client).toString()} color={getStatusClass(client.phoneNumberConfirmed)}
                        name="darkGreen avatar" size={128} />
                    <div>
                        <Subtitle1 block truncate wrap={false}>
                            <strong>ФИО:</strong> {client.firstName} {client.lastName} {client.middleName &&
                                <Subtitle1 as={"span"}>{client.middleName}</Subtitle1>}
                        </Subtitle1>
                        <Body1 as="p" block>
                            <strong>Статус:</strong> <Tag className={getStatusClass(client.phoneNumberConfirmed)} shape="circular"
                                appearance="outline"
                                size="extra-small">{getStatusText(client.phoneNumberConfirmed)}</Tag>

                        </Body1>
                        <Body1 as="p" block>
                            <strong>Дата регистрации:</strong> {new Date(client.createdAt).toLocaleDateString()}
                        </Body1>
                    </div>
                </div>

                <Field label="Электронная почта">
                    <div className="input-with-button">
                        <Input
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="Введите почту"
                            className="full-width-input"
                            type="email"

                        />
                        <Button appearance="outline" onClick={handleSubmit} icon={<CheckmarkFilled />} />
                    </div>
                </Field>


                <Field label="Телефон">
                    <div className="input-with-button">
                        <Input
                            value={phone}
                            onChange={(e) => setPhone(e.target.value)}
                            placeholder="Введите номер телефона"
                            className="full-width-input"
                            type="tel"

                        />
                        <Button appearance="outline" onClick={handleSubmit} icon={<CheckmarkFilled />} />
                    </div>
                </Field>



                <Field label="Дата рождения">
                    <Input
                        value={date ? date.toISOString().split("T")[0] : ""}
                        onChange={(e) => setDate(e.target.value ? new Date(e.target.value) : undefined)}
                        placeholder="Введите номер телефона"
                        type="date"
                    />
                </Field>

                <Field label="Пол">
                    <RadioGroup>
                        <Radio value="Мужчина" label="Мужчина" />
                        <Radio value="Женщина" label="Женщина" />
                        <Radio value="Другое" label="Другое" />
                    </RadioGroup>
                </Field>

                <Body2><strong>Номер карты доступа:</strong> {client.accessCardNumber}</Body2>

                <Button appearance="primary">
                    Скачать данные
                </Button>
            </div>


        </div>
    );
};