import type {FC} from "react";
import {Body1} from "@fluentui/react-components";
import {PhoneInput} from "@components/FormFields";
import {useComponentSize} from "@hooks/useComponentSize";

interface PhoneInputStepProps {
    phoneNumber: string;
    onPhoneChange: (value: string) => void;
    loading: boolean;
    externalError?: string;
}

export const PhoneInputStep: FC<PhoneInputStepProps> = ({
    phoneNumber,
    onPhoneChange,
    loading,
    externalError,
}) => {
    const { sizes } = useComponentSize();
    return (
        <>
            <Body1 block>
                Введите номер телефона, на который будет отправлен код подтверждения
            </Body1>
            <PhoneInput
                label="Номер телефона"
                required
                value={phoneNumber}
                onChange={onPhoneChange}
                placeholder="+7 (999) 123-45-67"
                disabled={loading}
                validateOnBlur
                externalError={externalError}
                size={sizes.input}
            />
        </>
    );
};
