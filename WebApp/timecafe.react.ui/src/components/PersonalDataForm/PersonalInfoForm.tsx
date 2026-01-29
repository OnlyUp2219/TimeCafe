import {useEffect, useState, type FC, createElement} from "react";
import {Card, Field, Input, Tag, RadioGroup, Radio, Button, Body2, Title2} from "@fluentui/react-components";
import {CheckmarkFilled, DismissFilled, Edit20Regular, type FluentIcon} from "@fluentui/react-icons";
import type {Gender, Profile} from "../../types/profile";
import {PhoneVerificationModal} from "../PhoneVerificationModal/PhoneVerificationModal.tsx";
import {DateInput, EmailInput} from "../FormFields";

interface PersonalInfoFormProps {
    profile: Profile;
    onChange?: (changed: Partial<Profile>) => void;
    onSave?: (data: Profile) => void;
    loading?: boolean;
    readOnly?: boolean;
    showDownloadButton?: boolean;
    className?: string;
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

export const PersonalInfoForm: FC<PersonalInfoFormProps> = ({
                                                                profile,
                                                                onChange,
                                                                onSave,
                                                                loading = false,
                                                                readOnly = false,
                                                                showDownloadButton = true,
                                                                className,
                                                            }) => {
    const normalizeDate = (value: unknown): Date | undefined => {
        if (!value) return undefined;
        if (value instanceof Date) return Number.isNaN(value.getTime()) ? undefined : value;
        if (typeof value === "string" || typeof value === "number") {
            const d = new Date(value);
            return Number.isNaN(d.getTime()) ? undefined : d;
        }
        return undefined;
    };

    const [email, setEmail] = useState(profile.email);
    const [phone, setPhone] = useState(profile.phoneNumber || "");
    const [birthDate, setBirthDate] = useState<Date | undefined>(() => normalizeDate(profile.birthDate));
    const [genderId, setGenderId] = useState<number | undefined>(profile.gender);
    const [showPhoneModal, setShowPhoneModal] = useState(false);

    useEffect(() => {
        setEmail(profile.email);
        setPhone(profile.phoneNumber || "");
        setBirthDate(normalizeDate(profile.birthDate));
        setGenderId(profile.gender);
    }, [profile.email, profile.phoneNumber, profile.birthDate, profile.gender]);

    const toIsoStringOrUndefined = (value: Date | undefined): string | undefined => {
        if (!value) return undefined;
        const t = value.getTime();
        if (Number.isNaN(t)) return undefined;
        return value.toISOString();
    };


    const handleSave = () => {
        const updated: Profile = {
            ...profile,
            email,
            phoneNumber: phone,
            birthDate: toIsoStringOrUndefined(birthDate),
            gender: ((genderId ?? 0) as Gender),
        };
        onSave?.(updated);
        onChange?.(updated);
    };

    const handlePhoneVerified = (verifiedPhone: string) => {
        setPhone(verifiedPhone);
        onChange?.({phoneNumber: verifiedPhone, phoneNumberConfirmed: true});
    };

    return (
        <Card className={className}>
            <Title2>Персональные данные</Title2>
            <div className="flex w-full gap-[12px] justify-stretch flex-wrap flex-row">
                <div className="flex-1">
                    <EmailInput
                        value={email ?? ""}
                        onChange={(value) => {
                            setEmail(value);
                            onChange?.({email: value});
                        }}
                        disabled={readOnly || loading}
                        shouldValidate={false}
                        trailingElement={
                            <Tag
                                appearance="outline"
                                        icon={createElement(getStatusIcon(profile.emailConfirmed))}
                                        className={`custom-tag ${getStatusClass(profile.emailConfirmed)}`}
                            />
                        }
                    />

                    <Field label="Телефон">
                        <div className="flex flex-col gap-2">
                            <div className="input-with-button">
                                <Input
                                    value={phone}
                                    onChange={(_, data) => {
                                        setPhone(data.value);
                                        onChange?.({phoneNumber: data.value});
                                    }}
                                    placeholder="Введите номер телефона"
                                    className="w-full"
                                    type="tel"
                                    disabled={readOnly || loading}
                                />
                                <Tag
                                    appearance="outline"
                                    icon={createElement(getStatusIcon(profile.phoneNumberConfirmed))}
                                    className={`custom-tag ${getStatusClass(profile.phoneNumberConfirmed)}`}
                                />
                            </div>

                            {!readOnly && (
                                <div className="flex justify-end">
                                    <Button
                                        appearance="subtle"
                                        size="small"
                                        icon={<Edit20Regular />}
                                        onClick={() => setShowPhoneModal(true)}
                                        disabled={loading || !phone.trim()}
                                    >
                                        {profile.phoneNumberConfirmed ? "Изменить телефон" : "Подтвердить телефон"}
                                    </Button>
                                </div>
                            )}
                        </div>
                    </Field>
                </div>

                <div className="flex-1">
                    <DateInput
                        value={birthDate}
                        onChange={(val) => {
                            setBirthDate(val);
                            onChange?.({birthDate: toIsoStringOrUndefined(val)});
                        }}
                        disabled={readOnly || loading}
                        label="Дата рождения"
                    />

                    <Field label="Пол">
                        <RadioGroup
                            value={genderId?.toString() || ""}
                            onChange={(_, data) => {
                                const g = data.value ? parseInt(data.value, 10) : undefined;
                                setGenderId(g);
                                onChange?.({gender: ((g ?? 0) as Gender)});
                            }}
                        >
                            <Radio value="1" label="Мужчина"/>
                            <Radio value="2" label="Женщина"/>
                            <Radio value="3" label="Другое"/>
                        </RadioGroup>
                    </Field>
                </div>
            </div>

            {profile.accessCardNumber && (
                <Body2 className="mt-[8px]"><strong>Номер карты доступа:</strong> {profile.accessCardNumber}</Body2>
            )}

            <div className="flex gap-[12px] mt-[16px]">
                {showDownloadButton && (
                    <Button appearance="primary" disabled>
                        Скачать данные
                    </Button>
                )}
                <Button appearance="secondary" disabled={loading || readOnly} onClick={handleSave}>
                    Сохранить изменения
                </Button>
            </div>

            <PhoneVerificationModal
                isOpen={showPhoneModal}
                onClose={() => setShowPhoneModal(false)}
                currentPhoneNumber={phone}
                currentPhoneNumberConfirmed={profile.phoneNumberConfirmed === true}
                onSuccess={handlePhoneVerified}
                mode="ui"
            />
        </Card>
    );
};
