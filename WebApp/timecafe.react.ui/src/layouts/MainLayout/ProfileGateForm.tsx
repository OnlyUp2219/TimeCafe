import type {FC} from "react";
import {Body1, Caption1, Field, Input, Radio, RadioGroup} from "@fluentui/react-components";
import {DateInput, PhoneInput} from "@components/FormFields";
import type {Profile} from "@app-types/profile";

interface ProfileGateFormProps {
    profile: Profile;
    firstName: string;
    lastName: string;
    middleName: string;
    birthDate: Date | undefined;
    genderId: 0 | 1 | 2;
    phoneDraft: string;
    email: string;
    saveError: string | null;
    disabled: boolean;
    onFirstNameChange: (val: string) => void;
    onLastNameChange: (val: string) => void;
    onMiddleNameChange: (val: string) => void;
    onBirthDateChange: (val: Date | undefined) => void;
    onGenderChange: (val: 0 | 1 | 2) => void;
    onPhoneChange: (val: string) => void;
    onPhoneValidationChange: (err: string | null) => void;
    onSaveErrorClear: () => void;
}

export const ProfileGateForm: FC<ProfileGateFormProps> = ({
    firstName,
    lastName,
    middleName,
    birthDate,
    genderId,
    phoneDraft,
    email,
    saveError,
    disabled,
    onFirstNameChange,
    onLastNameChange,
    onMiddleNameChange,
    onBirthDateChange,
    onGenderChange,
    onPhoneChange,
    onPhoneValidationChange,
    onSaveErrorClear,
}) => (
    <div className="flex flex-col gap-3">
        <Body1>
            Это обязательный шаг. Для сохранения нужны обязательные поля (имя и фамилия), остальные данные
            желательно заполнить сейчас.
        </Body1>

        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Field label="Фамилия" required>
                <Input data-testid="profile-gate-last-name" value={lastName} onChange={(_, d) => onLastNameChange(d.value)}/>
            </Field>
            <Field label="Имя" required>
                <Input data-testid="profile-gate-first-name" value={firstName} onChange={(_, d) => onFirstNameChange(d.value)}/>
            </Field>
            <Field label="Отчество">
                <Input value={middleName} onChange={(_, d) => onMiddleNameChange(d.value)}/>
            </Field>

            <DateInput
                value={birthDate}
                onChange={onBirthDateChange}
                disabled={disabled}
                label="Дата рождения"
                required={false}
                maxDate={new Date()}
            />
        </div>

        <Field label="Пол">
            <RadioGroup
                value={String(genderId)}
                disabled={disabled}
                onChange={(_, data) => {
                    const next = Number.parseInt(data.value, 10);
                    onGenderChange(next === 1 || next === 2 ? (next as 1 | 2) : 0);
                }}
            >
                <Radio value="0" label="Не указан"/>
                <Radio value="1" label="Мужчина"/>
                <Radio value="2" label="Женщина"/>
            </RadioGroup>
        </Field>

        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Field label="Email">
                <Input value={email} disabled/>
            </Field>

            <PhoneInput
                value={phoneDraft}
                onChange={(value) => {
                    onPhoneChange(value);
                    onSaveErrorClear();
                }}
                disabled={disabled}
                label="Телефон"
                required={false}
                validateOnBlur
                onValidationChange={(err) => onPhoneValidationChange(err ? err : null)}
            />
        </div>

        {saveError ? <Caption1 className="text-red-600">{saveError}</Caption1> : null}
    </div>
);
