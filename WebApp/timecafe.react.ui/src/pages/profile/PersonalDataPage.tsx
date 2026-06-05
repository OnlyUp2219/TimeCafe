import {
    Badge,
    Body2,
    Button,
    Card,
    Divider,
    MessageBar,
    MessageBarActions,
    MessageBarBody,
    MessageBarTitle,
    Title2,
    Subtitle2,
    Dialog,
    DialogSurface,
    DialogBody,
    DialogTitle,
    DialogContent,
} from "@fluentui/react-components";
import { useCallback, useState } from "react";
import { useAppDispatch, useAppSelector } from "@store/hooks";
import { useNavigate } from "react-router-dom";
import { ChangePasswordForm } from "@components/PersonalDataForm/ChangePasswordForm";
import { Gender, type Profile } from "@app-types/profile";
import { useProgressToast } from "@components/ToastProgress/ToastProgress";
import { PersonalDataMainForm } from "@components/PersonalDataForm/PersonalDataMainForm";
import { PhoneFormCard } from "@components/PersonalDataForm/PhoneFormCard";
import { EmailFormCard } from "@components/PersonalDataForm/EmailFormCard";
import { LogoutCard } from "@components/PersonalDataForm/LogoutCard";
import { StarRegular, DismissRegular, LockClosedRegular, PasswordFilled } from "@fluentui/react-icons";
import { clearTokens } from "@store/authSlice";
import { useLogoutMutation } from "@store/api/authApi";
import {
    useGetProfileByUserIdQuery,
    useUpdateProfileMutation,
    useUploadProfilePhotoMutation,
    useDeleteProfilePhotoMutation,
} from "@store/api/profileApi";
import { useGetUserLoyaltyQuery } from "@store/api/venueApi";
import { LoyaltyProgress } from "@components/Loyalty/LoyaltyProgress";
import { getUserMessageFromUnknown } from "@api/errors/getUserMessageFromUnknown";
import { normalizeBirthDateForApi } from "@utility/normalizeDate";
import { useComponentSize } from "@hooks/useComponentSize";

import "./PersonalData.css";

export const PersonalDataPage = () => {
    const { sizes } = useComponentSize();
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const userId = useAppSelector((state) => state.auth.userId);
    const { data: profile } = useGetProfileByUserIdQuery(userId, { skip: !userId });
    const { data: loyalty, isLoading: loyaltyLoading } = useGetUserLoyaltyQuery(userId, { skip: !userId });

    const { showToast, ToasterElement } = useProgressToast();
    const [logoutMutation] = useLogoutMutation();
    const [updateProfileMutation, { isLoading: saving }] = useUpdateProfileMutation();
    const [uploadPhoto] = useUploadProfilePhotoMutation();
    const [deletePhoto] = useDeleteProfilePhotoMutation();

    const [lastSaveError, setLastSaveError] = useState<string | null>(null);
    const [photoMessage, setPhotoMessage] = useState<string | null>(null);
    const [photoMessageIntent, setPhotoMessageIntent] = useState<"success" | "error">("success");

    const savePatch = useCallback(
        async (patch: Partial<Profile>, successMessage: string): Promise<boolean> => {
            if (!profile || !userId) return false;
            const merged = { ...profile, ...patch };
            try {
                await updateProfileMutation({
                    userId,
                    firstName: merged.firstName ?? "",
                    lastName: merged.lastName ?? "",
                    middleName: merged.middleName ?? null,
                    photoUrl: merged.photoUrl ?? null,
                    birthDate: normalizeBirthDateForApi(merged.birthDate),
                    gender: merged.gender ?? Gender.NotSpecified,
                }).unwrap();
                showToast(successMessage, "success", "Готово");
                setLastSaveError(null);
                return true;
            } catch (err) {
                const message = getUserMessageFromUnknown(err) || "Не удалось сохранить профиль.";
                showToast(message, "error", "Ошибка");
                setLastSaveError(message);
                return false;
            }
        },
        [profile, userId, updateProfileMutation, showToast]
    );

    const backgroundOpacityClass = profile ? "opacity-[0.08]" : "opacity-[0.1]";

    const [showPasswordForm, setShowPasswordForm] = useState(false);
    const [photoBusy, setPhotoBusy] = useState(false);

    const handleLogout = useCallback(async () => {
        await logoutMutation();
        dispatch(clearTokens());
        navigate("/login", { replace: true });
    }, [dispatch, logoutMutation, navigate]);

    const handlePhotoUpload = useCallback(async (file: File) => {
        if (!profile) return false;
        setPhotoBusy(true);
        try {
            await uploadPhoto({ userId, file }).unwrap();
            setPhotoMessage("Фото профиля обновлено.");
            setPhotoMessageIntent("success");
            return true;
        } catch (err) {
            setPhotoMessage(getUserMessageFromUnknown(err) || "Не удалось загрузить фото.");
            setPhotoMessageIntent("error");
            return false;
        } finally {
            setPhotoBusy(false);
        }
    }, [profile, uploadPhoto, userId]);

    const handlePhotoDelete = useCallback(async () => {
        if (!profile) return false;
        setPhotoBusy(true);
        try {
            await deletePhoto(userId).unwrap();
            setPhotoMessage("Фото профиля удалено.");
            setPhotoMessageIntent("success");
            return true;
        } catch (err) {
            setPhotoMessage(getUserMessageFromUnknown(err) || "Не удалось удалить фото.");
            setPhotoMessageIntent("error");
            return false;
        } finally {
            setPhotoBusy(false);
        }
    }, [deletePhoto, profile, userId]);

    if (!profile) {
        return (
            <div className="relative min-h-full">
                {ToasterElement}
                <div className="mx-auto w-full max-w-3xl px-2 py-4 sm:px-3 sm:py-6">
                    <Title2>Персональные данные</Title2>
                    <div>
                        Профиль не загружен. Перейдите на главную и попробуйте снова.
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="page-content flex flex-col gap-4">
            {ToasterElement}
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                    <div className="flex flex-col gap-1 min-w-0">
                        <Title2>Профиль</Title2>
                        <Body2>
                            Обновляйте контакты и основные данные — они используются для уведомлений и
                            восстановления доступа.
                        </Body2>
                    </div>
                </div>

                <Divider />

                {photoMessage && (
                    <MessageBar intent={photoMessageIntent}>
                        <MessageBarBody>
                            <MessageBarTitle>
                                {photoMessageIntent === "success" ? "Готово" : "Ошибка"}
                            </MessageBarTitle>
                            {photoMessage}
                        </MessageBarBody>
                        <MessageBarActions
                            containerAction={
                                <Button
                                    appearance="transparent"
                                    aria-label="Закрыть"
                                    icon={<DismissRegular className="size-5" />}
                                    onClick={() => setPhotoMessage(null)}
                                />
                            }
                        />
                    </MessageBar>
                )}

                {lastSaveError && (
                    <MessageBar intent="error">
                        <MessageBarBody>
                            <MessageBarTitle>Последняя ошибка сохранения</MessageBarTitle>
                            {lastSaveError}
                        </MessageBarBody>
                        <MessageBarActions
                            containerAction={
                                <Button
                                    appearance="transparent"
                                    aria-label="Закрыть"
                                    icon={<DismissRegular className="size-5" />}
                                    onClick={() => setLastSaveError(null)}
                                />
                            }
                        />
                    </MessageBar>
                )}

                <div className="flex flex-col gap-4">
                    <PersonalDataMainForm
                        profile={profile}
                        loading={saving}
                        onPhotoUrlChange={() => { }}
                        onPhotoUpload={handlePhotoUpload}
                        onPhotoDelete={handlePhotoDelete}
                        photoBusy={photoBusy}
                        onSave={(patch) => {
                            void savePatch(patch, "Персональные данные сохранены.");
                        }}
                    />

                    <Card className="w-full" size={sizes.card}>
                        <Title2 block className="!flex items-center gap-2">
                            <StarRegular className="text-(--colorBrandForeground1)" fontSize={24} />
                            Программа лояльности
                        </Title2>
                        <LoyaltyProgress
                            visitCount={profile?.visitCount || 0}
                            currentDiscount={loyalty?.personalDiscountPercent || 0}
                        />
                    </Card>

                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-2 ">
                        <PhoneFormCard
                            loading={saving}
                        />

                        <EmailFormCard
                            loading={saving}
                        />

                        <Card className="sm:col-span-2 lg:col-span-1" size={sizes.card}>
                            <Title2 block className="!flex items-center gap-2">
                                <LockClosedRegular className="text-(--colorBrandForeground1)" fontSize={24} />
                                Смена пароля
                            </Title2>
                            <Body2 className="!line-clamp-2">
                                Рекомендуется менять пароль регулярно и использовать уникальные пароли для
                                разных сервисов.
                            </Body2>
                            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-start mt-2">
                                <Button appearance="primary" icon={<PasswordFilled className="size-5" />}
                                    onClick={() => setShowPasswordForm(true)} size={sizes.button}>
                                    Сменить пароль
                                </Button>
                            </div>

                            {showPasswordForm && (
                                <Dialog open={showPasswordForm} onOpenChange={(_, data) => setShowPasswordForm(data.open)}>
                                    <DialogSurface>
                                        <DialogBody>
                                            <DialogTitle>Смена пароля</DialogTitle>
                                            <DialogContent className="pt-4">
                                                <ChangePasswordForm
                                                    wrapInCard={false}
                                                    showTitle={false}
                                                    redirectToLoginOnSuccess={false}
                                                    autoClearTokensOnSuccess={false}
                                                    showCancelButton
                                                    onCancel={() => setShowPasswordForm(false)}
                                                />
                                            </DialogContent>
                                        </DialogBody>
                                    </DialogSurface>
                                </Dialog>
                            )}
                        </Card>

                        <LogoutCard className="h-full border-2 border-dashed sm:col-span-2 lg:col-span-1"
                            onLogout={handleLogout} />
                    </div>

                </div>

        </div>
    );
};
