import { NO_DATA } from "@shared/const/placeholders";
import {useEffect, useId, useMemo, useState, type FC} from "react";
import {Button, Card, Field, Input, Label, Radio, RadioGroup, Text, Title2} from "@fluentui/react-components";
import {Edit20Filled, PersonRegular} from "@fluentui/react-icons";
import type {Profile} from "@app-types/profile";
import {DateInput} from "@components/FormFields";
import {ProfilePhotoCard} from "@components/ProfilePhotoCard/ProfilePhotoCard";
import {normalizeDate} from "@utility/normalizeDate";
import {useComponentSize} from "@hooks/useComponentSize";

export interface PersonalDataMainFormProps {
    profile: Profile;
    loading?: boolean;
    className?: string;
    onSave?: (patch: Partial<Profile>) => void;
    onPhotoUrlChange?: (url: string | null) => void;
    onPhotoUpload?: (file: File) => Promise<boolean>;
    onPhotoDelete?: () => Promise<boolean>;
    photoBusy?: boolean;
}

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
                                                                        onPhotoUpload,
                                                                        onPhotoDelete,
                                                                        photoBusy = false,
                                                                    }) => {
    const { sizes } = useComponentSize();
    const maxBirthDate = useMemo(() => new Date(), []);

    const [firstName, setFirstName] = useState(profile.firstName ?? "");
    const [lastName, setLastName] = useState(profile.lastName ?? "");
    const [middleName, setMiddleName] = useState(profile.middleName ?? "");
    const [birthDate, setBirthDate] = useState<Date | undefined>(() => normalizeDate(profile.birthDate));
    const [genderId, setGenderId] = useState<0 | 1 | 2>(() => normalizeGenderId(profile.gender));
    const [mode, setMode] = useState<"view" | "edit">("view");
    const genderGroupId = useId();
    const genderLabelId = useId();

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

    const displayName = `${profile.firstName?.trim() ?? ""} ${profile.lastName?.trim() ?? ""}`.trim() || profile.email || "Пользователь";

    const fullName = `${firstName} ${lastName}${middleName ? ` ${middleName}` : ""}`.trim();
    const genderText = genderId === 1 ? "Мужчина" : genderId === 2 ? "Женщина" : "Не указан";
    const birthDateText = birthDate ? birthDate.toLocaleDateString() : NO_DATA;

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
        <Card className={className} size={sizes.card}>
            <Title2 block className="!flex items-center gap-2">
                <PersonRegular className="text-(--colorBrandForeground1)" fontSize={24} />
                Персональные данные
            </Title2>

            <div>
                {!isEditing ? (
                    <div className="flex flex-col gap-3">
                        <div className="flex items-center gap-4">
                            <ProfilePhotoCard
                                displayName={displayName}
                                onPhotoUrlChange={onPhotoUrlChange}
                                onUpload={onPhotoUpload}
                                onDelete={onPhotoDelete}
                                busy={photoBusy}
                                asCard={false}
                                showTitle={false}
                                variant="view"
                                initialPhotoUrl={profile.photoUrl ?? null}
                            />

                            <div className="flex flex-col gap-1">
                                <Text weight="semibold" size={400}>{fullName || NO_DATA}</Text>
                                <Text>Пол: {genderText}</Text>
                                <Text>Дата рождения: {birthDateText}</Text>
                            </div>
                        </div>

                        <div className="flex flex-col sm:items-center sm:flex-row flex-wrap gap-2 sm:justify-end">
                            <Button
                                appearance="primary"
                                icon={<Edit20Filled/>}
                                onClick={() => setMode("edit")}
                                disabled={loading}
                                size={sizes.button}
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
                                onUpload={onPhotoUpload}
                                onDelete={onPhotoDelete}
                                busy={photoBusy}
                                asCard={false}
                                showTitle={false}
                                variant="edit"
                                disabled={loading || photoBusy}
                                initialPhotoUrl={profile.photoUrl ?? null}
                            />
                        </div>

                        <div className="flex flex-col gap-3">
                            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                                <Field label="Фамилия" size={sizes.field}>
                                    <Input value={lastName} onChange={(_, d) => setLastName(d.value)}
                                           disabled={loading} size={sizes.input}/>
                                </Field>
                                <Field label="Имя" size={sizes.field}>
                                    <Input value={firstName} onChange={(_, d) => setFirstName(d.value)}
                                           disabled={loading} size={sizes.input}/>
                                </Field>
                                <Field label="Отчество" size={sizes.field}>
                                    <Input value={middleName} onChange={(_, d) => setMiddleName(d.value)}
                                           disabled={loading} size={sizes.input}/>
                                </Field>

                                <DateInput
                                    value={birthDate}
                                    onChange={setBirthDate}
                                    disabled={loading}
                                    label="Дата рождения"
                                    maxDate={maxBirthDate}
                                    size={sizes.input}
                                />
                            </div>

                            <div className="flex flex-col gap-2">
                                <Label id={genderLabelId}>Пол</Label>
                                <RadioGroup
                                    id={genderGroupId}
                                    aria-labelledby={genderLabelId}
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
                            </div>

                            <div className="flex flex-wrap gap-2 sm:justify-end">
                                <Button appearance="primary" onClick={handleSave} disabled={loading} size={sizes.button}>
                                    Сохранить изменения
                                </Button>
                                <Button appearance="secondary" onClick={handleCancel} disabled={loading} size={sizes.button}>
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


