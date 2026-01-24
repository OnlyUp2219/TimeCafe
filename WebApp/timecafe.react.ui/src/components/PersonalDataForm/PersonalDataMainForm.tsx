import {useEffect, useState, type FC} from "react";
import {Button, Card, Field, Input, Radio, RadioGroup, Text, Title2} from "@fluentui/react-components";
import {Edit20Filled, PersonRegular} from "@fluentui/react-icons";
import type {ClientInfo} from "../../types/client.ts";
import {DateInput} from "../FormFields";
import {ProfilePhotoCard} from "../ProfilePhotoCard/ProfilePhotoCard";

export interface PersonalDataMainFormProps {
    client: ClientInfo;
    loading?: boolean;
    className?: string;
    onSave?: (patch: Partial<ClientInfo>) => void;
    onPhotoUrlChange?: (url: string | null) => void;
}

const normalizeDate = (value: unknown): Date | undefined => {
    if (!value) return undefined;
    if (value instanceof Date) return Number.isNaN(value.getTime()) ? undefined : value;
    if (typeof value === "string" || typeof value === "number") {
        const d = new Date(value);
        return Number.isNaN(d.getTime()) ? undefined : d;
    }
    return undefined;
};

const normalizeGenderId = (value: unknown): 0 | 1 | 2 => {
    const num = typeof value === "number" ? value : undefined;
    if (num === 1 || num === 2 || num === 0) return num;
    return 0;
};

export const PersonalDataMainForm: FC<PersonalDataMainFormProps> = ({
                                                                        client,
                                                                        loading = false,
                                                                        className,
                                                                        onSave,
                                                                        onPhotoUrlChange,
                                                                    }) => {

    const [firstName, setFirstName] = useState(client.firstName ?? "");
    const [lastName, setLastName] = useState(client.lastName ?? "");
    const [middleName, setMiddleName] = useState(client.middleName ?? "");
    const [birthDate, setBirthDate] = useState<Date | undefined>(() => normalizeDate(client.birthDate));
    const [genderId, setGenderId] = useState<0 | 1 | 2>(() => normalizeGenderId(client.genderId));
    const [mode, setMode] = useState<"view" | "edit">("view");

    useEffect(() => {
        setFirstName(client.firstName ?? "");
        setLastName(client.lastName ?? "");
        setMiddleName(client.middleName ?? "");
        setBirthDate(normalizeDate(client.birthDate));
        setGenderId(normalizeGenderId(client.genderId));
    }, [client.clientId, client.firstName, client.lastName, client.middleName, client.birthDate, client.genderId]);

    const displayName = `${client.lastName ?? ""} ${client.firstName ?? ""}${client.middleName ? ` ${client.middleName}` : ""}`.trim() || client.email;

    const fullName = `${lastName} ${firstName}${middleName ? ` ${middleName}` : ""}`.trim();
    const genderText = genderId === 1 ? "Мужчина" : genderId === 2 ? "Женщина" : "Не указан";
    const birthDateText = birthDate ? birthDate.toLocaleDateString() : "—";

    const handleSave = () => {
        onSave?.({
            firstName: firstName.trim(),
            lastName: lastName.trim(),
            middleName: middleName.trim() || undefined,
            birthDate,
            genderId,
        });
        setMode("view");
    };

    const handleCancel = () => {
        setFirstName(client.firstName ?? "");
        setLastName(client.lastName ?? "");
        setMiddleName(client.middleName ?? "");
        setBirthDate(normalizeDate(client.birthDate));
        setGenderId(normalizeGenderId(client.genderId));
        setMode("view");
    };

    const isEditing = mode === "edit";

    return (
        <Card className={className}>
            <Title2 block className="!flex gap-2">
                <div className="flex items-center gap-2 w-10 h-10 justify-center brand-badge rounded-full">
                    <PersonRegular/>
                </div>
                Персональные данные
            </Title2>

            <div>
                {!isEditing ? (
                    <div className="flex flex-col gap-3">
                        <div className="flex items-center gap-4">
                            <ProfilePhotoCard
                                displayName={displayName}
                                onPhotoUrlChange={onPhotoUrlChange}
                                asCard={false}
                                showTitle={false}
                                variant="view"
                                initialPhotoUrl={client.photo ?? null}
                            />

                            <div className="flex flex-col gap-1">
                                <Text weight="semibold" size={400}>{fullName || "—"}</Text>
                                <Text>Пол: {genderText}</Text>
                                <Text>Дата рождения: {birthDateText}</Text>
                                <Text>
                                    Номер карты
                                    доступа: {client.phoneNumberConfirmed === true ? (client.accessCardNumber ?? "—") : "—"}
                                </Text>
                            </div>
                        </div>

                        <div className="flex flex-col sm:items-center sm:flex-row flex-wrap gap-2 sm:justify-end">
                            <Button
                                appearance="primary"
                                icon={<Edit20Filled/>}
                                onClick={() => setMode("edit")}
                                disabled={loading}
                            >
                                Изменить
                            </Button>
                        </div>
                    </div>
                ) : (
                    <div className="grid grid-cols-1 gap-4 lg:grid-cols-[280px_1fr]">
                        <div>
                            <ProfilePhotoCard
                                displayName={displayName}
                                onPhotoUrlChange={onPhotoUrlChange}
                                asCard={false}
                                showTitle={false}
                                variant="edit"
                                disabled={loading}
                                initialPhotoUrl={client.photo ?? null}
                            />
                        </div>

                        <div className="flex flex-col gap-3">
                            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                                <Field label="Фамилия">
                                    <Input value={lastName} onChange={(_, d) => setLastName(d.value)}
                                           disabled={loading}/>
                                </Field>
                                <Field label="Имя">
                                    <Input value={firstName} onChange={(_, d) => setFirstName(d.value)}
                                           disabled={loading}/>
                                </Field>
                                <Field label="Отчество">
                                    <Input value={middleName} onChange={(_, d) => setMiddleName(d.value)}
                                           disabled={loading}/>
                                </Field>

                                <DateInput
                                    value={birthDate}
                                    onChange={setBirthDate}
                                    disabled={loading}
                                    label="Дата рождения"
                                />
                            </div>

                            <Field label="Пол">
                                <RadioGroup
                                    value={String(genderId)}
                                    disabled={loading}
                                    onChange={(_, data) => {
                                        const next = Number.parseInt(data.value, 10);
                                        setGenderId(next === 1 || next === 2 ? (next as 1 | 2) : 0);
                                    }}
                                >
                                    <Radio value="0" label="Не указан"/>
                                    <Radio value="1" label="Мужчина"/>
                                    <Radio value="2" label="Женщина"/>
                                </RadioGroup>
                            </Field>

                            <div>
                                <Text>
                                    Номер карты
                                    доступа: {client.phoneNumberConfirmed === true ? (client.accessCardNumber ?? "—") : "—"}
                                </Text>
                            </div>

                            <div className="flex flex-wrap gap-2 sm:justify-end">
                                <Button appearance="primary" onClick={handleSave} disabled={loading}>
                                    Сохранить изменения
                                </Button>
                                <Button appearance="secondary" onClick={handleCancel} disabled={loading}>
                                    Отмена
                                </Button>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </Card>
    );
};
