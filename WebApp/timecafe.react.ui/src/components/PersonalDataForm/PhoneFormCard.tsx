import {createElement, useEffect, useState, type FC} from "react";
import {
    Badge,
    Body1,
    Body1Strong,
    Body2,
    Button,
    Caption1,
    Card,
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
    Tag,
    Title2,
    Tooltip,
} from "@fluentui/react-components";
import {Delete20Regular, Edit20Filled, PhoneRegular} from "@fluentui/react-icons";
import {PhoneVerificationModal} from "@components/PhoneVerificationModal/PhoneVerificationModal";
import {
    isPhoneVerificationSessionV1,
    PHONE_VERIFICATION_SESSION_KEY,
    type PhoneVerificationSessionV1,
} from "@shared/auth/phoneVerificationSession";
import {useLocalStorageJson} from "@hooks/useLocalStorageJson";
import {useDispatch, useSelector} from "react-redux";
import type {RootState} from "@store";
import {authApi} from "@api/auth/authApi";
import {setPhoneNumber, setPhoneNumberConfirmed} from "@store/authSlice";
import {getUserMessageFromUnknown} from "@api/errors/getUserMessageFromUnknown";
import {hydrateAuthFromCurrentUser} from "@shared/auth/hydrateAuthFromCurrentUser";
import {getPersonalDataStatusClass, getPersonalDataStatusIcon} from "@components/PersonalDataForm/personalDataStatus";

export interface PhoneFormCardProps {
    loading?: boolean;
    className?: string;
}

export const PhoneFormCard: FC<PhoneFormCardProps> = ({loading = false, className}) => {
    const dispatch = useDispatch();
    const phoneNumber = useSelector((state: RootState) => state.auth.phoneNumber);
    const phoneNumberConfirmed = useSelector((state: RootState) => state.auth.phoneNumberConfirmed);
    const {load: loadPhoneSession} = useLocalStorageJson<PhoneVerificationSessionV1>(
        PHONE_VERIFICATION_SESSION_KEY,
        isPhoneVerificationSessionV1
    );

    const [showPhoneModal, setShowPhoneModal] = useState(false);
    const [phoneError, setPhoneError] = useState<string | null>(null);
    const [showClearDialog, setShowClearDialog] = useState(false);
    const [clearing, setClearing] = useState(false);

    const phone = phoneNumber || "";
    const hasPhone = Boolean(phone.trim());
    const confirmedForUi: boolean | null = hasPhone ? phoneNumberConfirmed : null;
    const actionLabel = !hasPhone ? "Заполнить" : phoneNumberConfirmed ? "Изменить" : "Подтвердить";

    useEffect(() => {
        const session = loadPhoneSession();
        if (session?.open) {
            setShowPhoneModal(true);
        }
    }, [loadPhoneSession]);

    const handlePhoneVerified = async () => {
        try {
            await hydrateAuthFromCurrentUser(dispatch);
        } catch {
            void 0;
        }
    };

    const handleClearPhone = async () => {
        setPhoneError(null);
        setClearing(true);
        try {
            await authApi.clearPhoneNumber();
            dispatch(setPhoneNumber(""));
            dispatch(setPhoneNumberConfirmed(false));
            setShowClearDialog(false);
        } catch (err: unknown) {
            setPhoneError(getUserMessageFromUnknown(err) || "Не удалось удалить номер телефона.");
        } finally {
            setClearing(false);
        }
    };

    return (
        <>
            <Card className={className}>
                <Title2 block className="!flex items-center gap-2">
                    <Badge appearance="tint" shape="rounded" size="extra-large" className="brand-badge">
                        <PhoneRegular className="size-5"/>
                    </Badge>
                    Телефон
                </Title2>
                <Body2 className="!line-clamp-2">
                    Используется для уведомлений и подтверждения номера.
                </Body2>

                <div>
                    <div className="flex flex-col gap-2">
                        <div className="flex flex-col sm:items-center sm:flex-row justify-between gap-2 min-w-0">
                            <div className="flex items-center gap-2 min-w-0 ">
                                <Tooltip content={`Телефон: ${phone || "не указан"}`} relationship="label">
                                    <Body1Strong className="!line-clamp-1 max-w-[25ch] md:max-w-[40ch] !truncate">
                                        {phone || "—"}
                                    </Body1Strong>
                                </Tooltip>
                                <Tooltip
                                    content={!hasPhone ? "Телефон не указан" : (phoneNumberConfirmed ? "Телефон подтверждён" : "Телефон не подтверждён")}
                                    relationship="description"
                                >
                                    <Tag
                                        appearance="outline"
                                        icon={createElement(getPersonalDataStatusIcon(confirmedForUi))}
                                        className={`custom-tag ${getPersonalDataStatusClass(confirmedForUi)}`}
                                    />
                                </Tooltip>
                            </div>

                            <div className="flex items-center gap-2">
                                <Button
                                    appearance="subtle"
                                    icon={<Delete20Regular/>}
                                    onClick={async () => {
                                        setShowClearDialog(true);
                                    }}
                                    disabled={loading || !hasPhone}
                                />
                                <Button
                                    appearance="primary"
                                    icon={<Edit20Filled/>}
                                    onClick={() => setShowPhoneModal(true)}
                                    disabled={loading}
                                >
                                    {actionLabel}
                                </Button>
                            </div>
                        </div>
                        {phoneError && <Caption1 className="text-red-600">{phoneError}</Caption1>}
                    </div>
                </div>

                <PhoneVerificationModal
                    isOpen={showPhoneModal}
                    onClose={() => setShowPhoneModal(false)}
                    currentPhoneNumber={phone}
                    currentPhoneNumberConfirmed={phoneNumberConfirmed}
                    onPhoneNumberSaved={(nextPhone) => {
                        dispatch(setPhoneNumber(nextPhone));
                        dispatch(setPhoneNumberConfirmed(false));
                    }}
                    onSuccess={async () => {
                        try {
                            await handlePhoneVerified();
                        } finally {
                            setShowPhoneModal(false);
                        }
                    }}
                />
            </Card>

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
        </>
    );
};
