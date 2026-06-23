import { NO_DATA } from "@shared/const/placeholders";
import {createElement, useState, type FC} from "react";
import {
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
    Title2,
    Tooltip,
} from "@fluentui/react-components";
import {Delete20Regular, Edit20Filled, PhoneRegular} from "@fluentui/react-icons";
import {PhoneVerificationModal} from "@components/PhoneVerificationModal/PhoneVerificationModal";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {useClearPhoneNumberMutation} from "@store/api/authApi";
import {setPhoneNumber, setPhoneNumberConfirmed} from "@store/authSlice";
import {getUserMessageFromUnknown} from "@api/errors/getUserMessageFromUnknown";
import {hydrateAuthFromCurrentUser} from "@shared/auth/hydrateAuthFromCurrentUser";
import {getPersonalDataStatusIcon} from "@components/PersonalDataForm/personalDataStatus";
import {useComponentSize} from "@hooks/useComponentSize";
import {profileApi} from "@store/api/profileApi";

export interface PhoneFormCardProps {
    loading?: boolean;
    className?: string;
}

export const PhoneFormCard: FC<PhoneFormCardProps> = ({loading = false, className}) => {
    const { sizes } = useComponentSize();
    const dispatch = useAppDispatch();
    const [clearPhoneMutation] = useClearPhoneNumberMutation();
    const phoneNumber = useAppSelector((state) => state.auth.phoneNumber);
    const phoneNumberConfirmed = useAppSelector((state) => state.auth.phoneNumberConfirmed);
    const userId = useAppSelector((state) => state.auth.userId);

    const [showPhoneModal, setShowPhoneModal] = useState(false);
    const [phoneError, setPhoneError] = useState<string | null>(null);
    const [showClearDialog, setShowClearDialog] = useState(false);
    const [clearing, setClearing] = useState(false);

    const phone = phoneNumber || "";
    const hasPhone = Boolean(phone.trim());
    const confirmedForUi: boolean | null = hasPhone ? phoneNumberConfirmed : null;
    const actionLabel = !hasPhone ? "Заполнить" : phoneNumberConfirmed ? "Изменить" : "Подтвердить";

    const handlePhoneVerified = async () => {
        try {
            await hydrateAuthFromCurrentUser(dispatch);
            if (userId) {
                dispatch(profileApi.util.invalidateTags([{ type: "Profile", id: userId }]));
            }
        } catch {
            void 0;
        }
    };

    const handleClearPhone = async () => {
        setPhoneError(null);
        setClearing(true);
        try {
            await clearPhoneMutation().unwrap();
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
            <Card className={className} size={sizes.card}>
                <Title2 block className="!flex items-center gap-2">
                    <PhoneRegular className="text-(--colorBrandForeground1)" fontSize={24} />
                    Телефон
                </Title2>
                <Body2 className="!line-clamp-2">
                    Используется для уведомлений и подтверждения номера.
                </Body2>

                <div>
                    <div className="flex flex-col gap-2">
                        <div className="flex flex-col sm:items-center sm:flex-row justify-between gap-2 min-w-0">
                            <div className="flex items-center gap-2 min-w-0 flex-wrap">
                                <Tooltip content={`Телефон: ${phone || "не указан"}`} relationship="label">
                                    <Body1Strong className="!line-clamp-1 max-w-[25ch] md:max-w-[40ch] !truncate">
                                        {phone || NO_DATA}
                                    </Body1Strong>
                                </Tooltip>
                                <div className="flex items-center gap-1.5 sm:ml-2 shrink-0">
                                    {createElement(getPersonalDataStatusIcon(confirmedForUi), {
                                        className: confirmedForUi ? "text-(--colorStatusSuccessForeground1)" : (confirmedForUi === false ? "text-(--colorStatusDangerForeground1)" : "text-(--colorNeutralForeground3)"),
                                        fontSize: 16
                                    })}
                                    <Caption1 className={confirmedForUi ? "text-(--colorStatusSuccessForeground1)" : (confirmedForUi === false ? "text-(--colorStatusDangerForeground1)" : "text-(--colorNeutralForeground3)")}>
                                        {!hasPhone ? "Не указан" : (phoneNumberConfirmed ? "Подтверждён" : "Не подтверждён")}
                                    </Caption1>
                                </div>
                            </div>

                            <div className="flex flex-row items-center gap-2 w-full sm:w-auto">
                                <Button
                                    appearance="subtle"
                                    icon={<Delete20Regular/>}
                                    onClick={async () => {
                                        setShowClearDialog(true);
                                    }}
                                    disabled={loading || !hasPhone}
                                    size={sizes.button}
                                />
                                <Button
                                    appearance="primary"
                                    icon={<Edit20Filled/>}
                                    onClick={() => setShowPhoneModal(true)}
                                    disabled={loading}
                                    size={sizes.button}
                                    className="flex-1 sm:flex-none"
                                >
                                    {actionLabel}
                                </Button>
                            </div>
                        </div>
                        {phoneError && <Caption1 className="text-(--colorPaletteRedForeground1)">{phoneError}</Caption1>}
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
                                size={sizes.button}
                            >
                                Отмена
                            </Button>
                            <Button
                                appearance="primary"
                                onClick={handleClearPhone}
                                disabled={clearing}
                                size={sizes.button}
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


