import {useState, type FC, createElement} from "react";
import {Card, Field, Input, Tag, RadioGroup, Radio, Button, Body2, Title2} from "@fluentui/react-components";
import {CheckmarkFilled, DismissFilled, type FluentIcon} from "@fluentui/react-icons";
import type {ClientInfo} from "../../types/client.ts";
import {PhoneVerificationModal} from "../PhoneVerificationModal/PhoneVerificationModal.tsx";

interface PersonalInfoFormProps {
    client: ClientInfo;
    onChange?: (changed: Partial<ClientInfo>) => void;
    onSave?: (data: ClientInfo) => void;
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
                                                                client,
                                                                onChange,
                                                                onSave,
                                                                loading = false,
                                                                readOnly = false,
                                                                showDownloadButton = true,
                                                                className,
                                                            }) => {
    const [email, setEmail] = useState(client.email);
    const [phone, setPhone] = useState(client.phoneNumber || "");
    const [birthDate, setBirthDate] = useState<Date | undefined>(client.birthDate);
    const [genderId, setGenderId] = useState<number | undefined>(client.genderId);
    const [showPhoneModal, setShowPhoneModal] = useState(false);

    const handleSave = () => {
        const updated: ClientInfo = {
            ...client,
            email,
            phoneNumber: phone,
            birthDate,
            genderId,
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
                    <Field label="Электронная почта" className="w-full">
                        <div className="input-with-button">
                            <Input
                                value={email}
                                onChange={(e) => {
                                    setEmail(e.target.value);
                                    onChange?.({email: e.target.value});
                                }}
                                placeholder="Введите почту"
                                className="w-full"
                                type="email"
                                disabled={readOnly || loading}
                            />
                            <Tag
                                appearance="outline"
                                icon={createElement(getStatusIcon(client.emailConfirmed))}
                                className={`custom-tag ${getStatusClass(client.emailConfirmed)}`}
                            />
                        </div>
                    </Field>

                    <Field label="Телефон">
                        <div className="input-with-button">
                            <Input
                                value={phone}
                                onChange={(e) => {
                                    setPhone(e.target.value);
                                    onChange?.({phoneNumber: e.target.value});
                                }}
                                placeholder="Введите номер телефона"
                                className="w-full"
                                type="tel"
                                disabled={readOnly || loading}
                            />
                            <Tag
                                appearance="outline"
                                icon={createElement(getStatusIcon(client.phoneNumberConfirmed))}
                                className={`custom-tag ${getStatusClass(client.phoneNumberConfirmed)}`}
                            />
                            <Button
                                appearance="subtle"
                                size="small"
                                onClick={() => setShowPhoneModal(true)}
                                disabled={readOnly || loading}
                            >
                                {client.phoneNumberConfirmed ? "Изменить" : "Подтвердить"}
                            </Button>
                        </div>
                    </Field>
                </div>

                <div className="flex-1">
                    <Field label="Дата рождения">
                        <Input
                            value={birthDate ? birthDate.toISOString().split("T")[0] : ""}
                            onChange={(e) => {
                                const val = e.target.value ? new Date(e.target.value) : undefined;
                                setBirthDate(val);
                                onChange?.({birthDate: val});
                            }}
                            placeholder="Введите дату рождения"
                            type="date"
                            className="w-full"
                            disabled={readOnly || loading}
                        />
                    </Field>

                    <Field label="Пол">
                        <RadioGroup
                            value={genderId?.toString() || ""}
                            onChange={(_, data) => {
                                const g = data.value ? parseInt(data.value, 10) : undefined;
                                setGenderId(g);
                                onChange?.({genderId: g});
                            }}
                        >
                            <Radio value="1" label="Мужчина"/>
                            <Radio value="2" label="Женщина"/>
                            <Radio value="3" label="Другое"/>
                        </RadioGroup>
                    </Field>
                </div>
            </div>

            {client.accessCardNumber && (
                <Body2 className="mt-[8px]"><strong>Номер карты доступа:</strong> {client.accessCardNumber}</Body2>
            )}

            <div className="flex gap-[12px] mt-[16px]">
                {showDownloadButton && (
                    <Button appearance="primary" disabled={loading || readOnly} onClick={() => {/* TODO: implement download */
                    }}>
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
                onSuccess={handlePhoneVerified}
            />
        </Card>
    );
};
