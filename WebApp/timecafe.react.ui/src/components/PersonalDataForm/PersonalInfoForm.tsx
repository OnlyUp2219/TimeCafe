import {useEffect, useMemo, useState, type FC, createElement} from "react";
import {
    Body1,
    Button,
    Caption1,
    Card,
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
    Field,
    Radio,
    RadioGroup,
    Tag,
    Title2,
} from "@fluentui/react-components";
import {CheckmarkFilled, Delete20Regular, DismissFilled, Edit20Regular, type FluentIcon} from "@fluentui/react-icons";
import type {Gender, Profile} from "@app-types/profile";
import {PhoneVerificationModal} from "@components/PhoneVerificationModal/PhoneVerificationModal";
import {DateInput, EmailInput, PhoneInput} from "@components/FormFields";
import {useDispatch, useSelector} from "react-redux";
import type {AppDispatch, RootState} from "@store";
import {setPhoneNumber, setPhoneNumberConfirmed} from "@store/authSlice";
import {authApi} from "@api/auth/authApi";
import {getUserMessageFromUnknown} from "@api/errors/getUserMessageFromUnknown";
import {normalizeDate} from "@utility/normalizeDate";

interface PersonalInfoFormProps {
    profile: Profile;
    onChange?: (changed: Partial<Profile>) => void;
    onSave?: (data: Profile) => void;
    loading?: boolean;
    readOnly?: boolean;
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
                                                                className,
                                                            }) => {
    const authEmailConfirmed = useSelector((state: RootState) => state.auth.emailConfirmed);
    const authPhoneConfirmed = useSelector((state: RootState) => state.auth.phoneNumberConfirmed);
    const dispatch = useDispatch<AppDispatch>();
    const maxBirthDate = useMemo(() => new Date(), []);
    const [email, setEmail] = useState(profile.email);
    const [phone, setPhone] = useState(profile.phoneNumber || "");
    const [birthDate, setBirthDate] = useState<Date | undefined>(() => normalizeDate(profile.birthDate));
    const [genderId, setGenderId] = useState<number | undefined>(profile.gender);
    const [showPhoneModal, setShowPhoneModal] = useState(false);
    const [phoneError, setPhoneError] = useState<string | null>(null);
    const [showClearDialog, setShowClearDialog] = useState(false);
    const [clearing, setClearing] = useState(false);

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

    const handleClearPhone = async () => {
        setPhoneError(null);
        setClearing(true);
        try {
            await authApi.clearPhoneNumber();
            setPhone("");
            dispatch(setPhoneNumber(""));
            dispatch(setPhoneNumberConfirmed(false));
            onChange?.({phoneNumber: "", phoneNumberConfirmed: false});
            setShowClearDialog(false);
        } catch (err: unknown) {
            setPhoneError(getUserMessageFromUnknown(err) || "Не удалось удалить номер телефона.");
        } finally {
            setClearing(false);
        }
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
                                icon={createElement(getStatusIcon(authEmailConfirmed))}
                                className={`custom-tag ${getStatusClass(authEmailConfirmed)}`}
                            />
                        }
                    />

                    <div className="flex flex-col gap-2">
                        <PhoneInput
                            value={phone}
                            onChange={(value) => {
                                setPhone(value);
                                onChange?.({phoneNumber: value});
                            }}
                            disabled={readOnly || loading}
                            required={false}
                            shouldValidate={false}
                            validateOnBlur
                            trailingElement={
                                <Tag
                                    appearance="outline"
                                    icon={createElement(getStatusIcon(authPhoneConfirmed))}
                                    className={`custom-tag ${getStatusClass(authPhoneConfirmed)}`}
                                />
                            }
                        />

                        {!readOnly && (
                            <div className="flex justify-end gap-2">
                                <Button
                                    appearance="subtle"
                                    size="small"
                                    icon={<Delete20Regular/>}
                                    onClick={async () => {
                                        setShowClearDialog(true);
                                    }}
                                    disabled={loading || !phone.trim()}
                                />
                                <Button
                                    appearance="subtle"
                                    size="small"
                                    icon={<Edit20Regular/>}
                                    onClick={() => setShowPhoneModal(true)}
                                    disabled={loading || !phone.trim()}
                                >
                                    {authPhoneConfirmed ? "Изменить телефон" : "Подтвердить телефон"}
                                </Button>
                            </div>
                        )}
                        {phoneError && <Caption1 className="text-red-600">{phoneError}</Caption1>}
                    </div>
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
                        maxDate={maxBirthDate}
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

            <div className="flex gap-[12px] mt-[16px]">
                <Button appearance="secondary" disabled={loading || readOnly} onClick={handleSave}>
                    Сохранить изменения
                </Button>
            </div>

            <PhoneVerificationModal
                isOpen={showPhoneModal}
                onClose={() => setShowPhoneModal(false)}
                currentPhoneNumber={phone}
                currentPhoneNumberConfirmed={authPhoneConfirmed}
                onPhoneNumberSaved={(nextPhone) => {
                    dispatch(setPhoneNumber(nextPhone));
                    dispatch(setPhoneNumberConfirmed(false));
                }}
                onSuccess={handlePhoneVerified}
            />
            <Dialog open={showClearDialog} modalType="alert">
                <DialogSurface>
                    <DialogBody>
                        <DialogTitle>Удалить номер телефона?</DialogTitle>
                        <DialogContent>
                            <Body1>
                                Без номера телефона вы не сможете оформить заказ и получать уведомления.
                            </Body1>
                        </DialogContent>
                        <DialogActions>
                            <Button
                                appearance="secondary"
                                onClick={() => setShowClearDialog(false)}
                                disabled={clearing}
                            >
                                Отмена
                            </Button>
                            <Button
                                appearance="primary"
                                onClick={handleClearPhone}
                                disabled={clearing}
                            >
                                Удалить
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>
        </Card>
    );
};
