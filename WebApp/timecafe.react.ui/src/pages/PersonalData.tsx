import * as React from "react";
import {
    Button,
    Text,
    Title2,
    Subtitle1,
    Image,
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
import {useState} from "react";
import {CheckmarkFilled} from '@fluentui/react-icons';

interface Client {
    clientId: number;
    firstName: string;
    lastName: string;
    middleName?: string;
    genderId?: number;
    email: string;
    birthDate?: string;
    phoneNumber: string;
    accessCardNumber: string;
    statusId?: number;
    refusalReason?: string;
    photo?: string;
    createdAt: string;
}

export const PersonalData: React.FC = () => {
    const client: Client = {
        clientId: 1,
        firstName: "Даниил",
        lastName: "Иванов",
        middleName: "Иванович",
        genderId: 1,
        email: "ivan@example.com",
        birthDate: "1990-01-01",
        phoneNumber: "+7 (999) 123-45-67",
        accessCardNumber: "123456789",
        statusId: 4,
        refusalReason: null,
        photo: "https://via.placeholder.com/200",
        createdAt: "2023-01-01T12:00:00",
    };

    const getInitials = (client: Client): string => {
        const parts = [
            client.firstName ? client.firstName[0] : "",
            client.lastName ? client.lastName[0] : "",
        ].filter(Boolean);
        return parts.join("");
    };

    const getStatusText = (statusId?: number): string => {
        const statusMap: { [key: number]: string } = {
            1: "Активный",
            2: "Требует активности",
            3: "Неактивный",
        };
        return statusMap[statusId] || "Ошибка";
    };


    const getStatusClass = (statusId?: number): string => {
        if (statusId === 1) return "dark-green";
        if (statusId === 2) return "pumpkin";
        if (statusId === 3) return "beige";
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
                    <Avatar initials={getInitials(client).toString()} color={getStatusClass(client.statusId)}
                            name="darkGreen avatar" size={128}/>
                    <div>
                        <Subtitle1 block truncate wrap={false}>
                            <strong>ФИО:</strong> {client.firstName} {client.lastName} {client.middleName &&
                            <Subtitle1 as={"span"}>{client.middleName}</Subtitle1>}
                        </Subtitle1>
                        <Body1 as="p" block>
                            <strong>Статус:</strong> <Tag className={getStatusClass(client.statusId)} shape="circular"
                                                          appearance="outline"
                                                          size="extra-small">{getStatusText(client.statusId)}</Tag>

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
                        <Button appearance="outline" onClick={handleSubmit} icon={<CheckmarkFilled/>}/>
                    </div>
                </Field>


                <Field label="Телефона">
                    <div className="input-with-button">
                        <Input
                            value={phone}
                            onChange={(e) => setPhone(e.target.value)}
                            placeholder="Введите номер телефона"
                            className="full-width-input"
                            type="tel"

                        />
                        <Button appearance="outline" onClick={handleSubmit} icon={<CheckmarkFilled/>}/>
                    </div>
                </Field>


                {client.birthDate && (

                    <Field label="Дата рождения">
                        <Input
                            value={date}
                            onChange={(e) => setDate(e.target.value)}
                            placeholder="Введите номер телефона"
                            type="date"
                        />
                    </Field>
                )}

                {client.genderId && (
                    <Field label="Пол">
                        <RadioGroup>
                            <Radio value="Мужчина" label="Мужчина"/>
                            <Radio value="Женщина" label="Женщина"/>
                            <Radio value="Другое" label="Другое"/>
                        </RadioGroup>
                    </Field>
                )}

                <Body2><strong>Номер карты доступа:</strong> {client.accessCardNumber}</Body2>

                <Button appearance="primary">
                    Скачать данные
                </Button>
            </div>


        </div>
    );
};