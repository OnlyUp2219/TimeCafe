import {useEffect, useState, type FC} from "react";
import {Badge, Button, Card, Field, Input, Radio, RadioGroup, Text, Title2} from "@fluentui/react-components";
import {Edit20Filled, PersonRegular} from "@fluentui/react-icons";
import type {Profile} from "../../types/profile";
import {DateInput} from "../FormFields";
import {ProfilePhotoCard} from "../ProfilePhotoCard/ProfilePhotoCard";

export interface PersonalDataMainFormProps {
    profile: Profile;
    loading?: boolean;
    className?: string;
    onSave?: (patch: Partial<Profile>) => void;
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
                                                                        profile,
                                                                        loading = false,
                                                                        className,
                                                                        onSave,
                                                                        onPhotoUrlChange,
                                                                    }) => {

    const [firstName, setFirstName] = useState(profile.firstName ?? "");
    const [lastName, setLastName] = useState(profile.lastName ?? "");
    const [middleName, setMiddleName] = useState(profile.middleName ?? "");
    const [birthDate, setBirthDate] = useState<Date | undefined>(() => normalizeDate(profile.birthDate));
    const [genderId, setGenderId] = useState<0 | 1 | 2>(() => normalizeGenderId(profile.gender));
    const [mode, setMode] = useState<"view" | "edit">("view");

    useEffect(() => {
        setFirstName(profile.firstName ?? "");
        setLastName(profile.lastName ?? "");
        setMiddleName(profile.middleName ?? "");
        setBirthDate(normalizeDate(profile.birthDate));
        setGenderId(normalizeGenderId(profile.gender));
    }, [profile.firstName, profile.lastName, profile.middleName, profile.birthDate, profile.gender]);

    const toIsoStringOrUndefined = (value: Date | undefined): string | undefined => {
        if (!value) return undefined;
        const t = value.getTime();
        if (Number.isNaN(t)) return undefined;
        return value.toISOString();
    };

    const displayName =
        `${profile.lastName ?? ""} ${profile.firstName ?? ""}${profile.middleName ? ` ${profile.middleName}` : ""}`.trim() ||
        profile.email ||
        "";

    const fullName = `${lastName} ${firstName}${middleName ? ` ${middleName}` : ""}`.trim();
    const genderText = genderId === 1 ? "Мужчина" : genderId === 2 ? "Женщина" : "Не указан";
    const birthDateText = birthDate ? birthDate.toLocaleDateString() : "—";

    const handleSave = () => {
        onSave?.({
            firstName: firstName.trim(),
            lastName: lastName.trim(),
            middleName: middleName.trim() || undefined,
            birthDate: toIsoStringOrUndefined(birthDate),
            gender: genderId,
        });
        setMode("view");
    };

    const handleCancel = () => {
        setFirstName(profile.firstName ?? "");
        setLastName(profile.lastName ?? "");
        setMiddleName(profile.middleName ?? "");
        setBirthDate(normalizeDate(profile.birthDate));
        setGenderId(normalizeGenderId(profile.gender));
        setMode("view");
    };

    const isEditing = mode === "edit";

    return (
        <Card className={className}>
            <Title2 block className="!flex items-center gap-2">
                <Badge appearance="tint" shape="rounded" size="extra-large" className="brand-badge">
                    <PersonRegular className="size-5" />
                </Badge>
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
                                initialPhotoUrl={profile.photoUrl ?? null}
                            />

                            <div className="flex flex-col gap-1">
                                <Text weight="semibold" size={400}>{fullName || "—"}</Text>
                                <Text>Пол: {genderText}</Text>
                                <Text>Дата рождения: {birthDateText}</Text>
                                <Text>
                                    Номер карты
                                    доступа: {profile.phoneNumberConfirmed === true ? (profile.accessCardNumber ?? "—") : "—"}
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
                                initialPhotoUrl={profile.photoUrl ?? null}
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
                                    доступа: {profile.phoneNumberConfirmed === true ? (profile.accessCardNumber ?? "—") : "—"}
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
