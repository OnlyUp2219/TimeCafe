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
} from "@fluentui/react-components";
import {useCallback, useState} from "react";
import {useDispatch, useSelector} from "react-redux";
import {useNavigate} from "react-router-dom";
import type {AppDispatch, RootState} from "../../store";
import {ChangePasswordForm} from "../../components/PersonalDataForm/ChangePasswordForm";
import type {Profile} from "../../types/profile";
import {setProfile, updateProfile} from "../../store/profileSlice";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress";
import {PersonalDataMainForm} from "../../components/PersonalDataForm/PersonalDataMainForm";
import {PhoneFormCard} from "../../components/PersonalDataForm/PhoneFormCard";
import {EmailFormCard} from "../../components/PersonalDataForm/EmailFormCard";
import {LogoutCard} from "../../components/PersonalDataForm/LogoutCard";
import {LockClosedRegular, PasswordFilled} from "@fluentui/react-icons";
import {DismissRegular} from "@fluentui/react-icons";
import {authApi} from "../../shared/api/auth/authApi";
import {clearTokens} from "../../store/authSlice";
import blob3Url from "../../assets/ssshape_blob3.svg";
import blob4Url from "../../assets/ssshape_blob4.svg";
import squiggly1Url from "../../assets/sssquiggl_1.svg";
import glitchUrl from "../../assets/ggglitch.svg";
import "./PersonalData.css";

export const PersonalDataPage = () => {
    const navigate = useNavigate();
    const dispatch = useDispatch<AppDispatch>();
    const profile = useSelector((state: RootState) => state.profile.data);
    const saving = useSelector((state: RootState) => state.profile.saving);

    const {showToast, ToasterElement} = useProgressToast();

    const [lastSaveError, setLastSaveError] = useState<string | null>(null);

    const savePatch = useCallback(
        async (patch: Partial<Profile>, successMessage: string): Promise<boolean> => {
            const action = await dispatch(updateProfile(patch));
            if (updateProfile.fulfilled.match(action)) {
                showToast(successMessage, "success", "Готово");
                setLastSaveError(null);
                return true;
            }

            const message =
                (updateProfile.rejected.match(action) && (action.payload as string | undefined)) ||
                "Не удалось сохранить профиль.";
            showToast(message, "error", "Ошибка");
            setLastSaveError(message);
            return false;
        },
        [dispatch, showToast]
    );

    const loadDemoProfile = useCallback(() => {
        const demo: Profile = {
            firstName: "Иван",
            lastName: "Иванов",
            middleName: "Иванович",
            email: "demo@timecafe.local",
            emailConfirmed: true,
            phoneNumber: "+375291234567",
            phoneNumberConfirmed: true,
            birthDate: "1999-01-15",
            gender: 1,
            accessCardNumber: "TC-000001",
            photoUrl: undefined,
            profileStatus: 0,
            banReason: undefined,
        };

        dispatch(setProfile(demo));
        showToast("Загружен демо-профиль для теста UI.", "info", "Demo");
    }, [dispatch, showToast]);

    const backgroundOpacityClass = profile ? "opacity-[0.08]" : "opacity-[0.1]";

    const [, setPhotoUrl] = useState<string | null | undefined>(undefined);
    const [showPasswordForm, setShowPasswordForm] = useState(false);

    const handleLogout = useCallback(async () => {
        await authApi.logout();
        dispatch(clearTokens());
        navigate("/login", {replace: true});
    }, [dispatch, navigate]);

    const content = !profile ? (
        <div className="mx-auto w-full max-w-3xl px-2 py-4 sm:px-3 sm:py-6 relative z-10">
            <div
                className="rounded-3xl p-6 personal-data-gradient-card"
            >
                <Title2>Персональные данные</Title2>
                <div>
                    Профиль не загружен. Перейдите на главную и попробуйте снова.
                </div>

                <div className="mt-4 flex gap-2">
                    <Button appearance="secondary" onClick={loadDemoProfile}>
                        Загрузить демо-профиль
                    </Button>
                </div>
            </div>
        </div>
    ) : (
        <div className="mx-auto  w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6 relative z-10 ">
            <div
                className="rounded-3xl p-5 sm:p-8 personal-data-gradient-card"
            >
                <div className="flex flex-col gap-4">
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                        <div className="flex flex-col gap-1 min-w-0">
                            <Title2>Профиль</Title2>
                            <Body2>
                                Обновляйте контакты и основные данные — они используются для уведомлений и
                                восстановления доступа.
                            </Body2>
                        </div>
                    </div>

                    <Divider/>

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
                        <>
                            <PersonalDataMainForm
                                profile={profile}
                                loading={saving}
                                onPhotoUrlChange={(url) => setPhotoUrl(url)}
                                onSave={(patch) => {
                                    void savePatch(patch, "Персональные данные сохранены.");
                                }}
                            />

                            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-2 ">
                                <PhoneFormCard
                                    profile={profile}
                                    loading={saving}
                                    onSave={(patch) => {
                                        void savePatch(patch, "Телефон сохранён.");
                                    }}
                                />

                                <EmailFormCard
                                    profile={profile}
                                    loading={saving}
                                    onSave={(patch) => {
                                        void savePatch(patch, "Почта сохранена.");
                                    }}
                                />

                                <Card className="sm:col-span-2 lg:col-span-1">
                                    <Title2 block className="!flex items-center gap-2">
                                        <Badge appearance="tint" shape="rounded" size="extra-large"
                                               className="brand-badge">
                                            <LockClosedRegular className="size-5"/>
                                        </Badge>
                                        Смена пароля
                                    </Title2>
                                    <Body2 className="!line-clamp-2">
                                        Рекомендуется менять пароль регулярно и использовать уникальные пароли для
                                        разных сервисов.
                                    </Body2>
                                    <div
                                        className={!showPasswordForm ? "flex flex-col sm:flex-row sm:items-center sm:justify-end " : "w-full"}>
                                        {!showPasswordForm ? (
                                            <Button appearance="primary" icon={<PasswordFilled className="size-5" />}
                                                    onClick={() => setShowPasswordForm(true)}>
                                                Сменить пароль
                                            </Button>
                                        ) : (
                                            <ChangePasswordForm
                                                wrapInCard={false}
                                                showTitle={false}
                                                mode="ui"
                                                redirectToLoginOnSuccess={false}
                                                autoClearTokensOnSuccess={false}
                                                showCancelButton
                                                onCancel={() => setShowPasswordForm(false)}
                                            />
                                        )}
                                    </div>
                                </Card>

                                <LogoutCard className="h-full border-2 border-dashed sm:col-span-2 lg:col-span-1"
                                            onLogout={handleLogout}/>
                            </div>
                        </>

                    </div>
                </div>
            </div>
        </div>
    );

    return (
        <div className="tc-noise-overlay relative overflow-hidden min-h-full">
            {ToasterElement}

            <div className="pointer-events-none absolute inset-0 z-0 overflow-hidden">
                <img
                    src={blob3Url}
                    alt=""
                    aria-hidden="true"
                    className={`absolute -top-[10vw] -right-[10vw] w-[50vw] max-w-[640px] rotate-6 select-none ${backgroundOpacityClass}`}
                    draggable={false}
                />
                <img
                    src={blob4Url}
                    alt=""
                    aria-hidden="true"
                    className={`absolute -left-[12vw] top-[35vh] w-[55vw] max-w-[720px] -rotate-6 select-none ${backgroundOpacityClass}`}
                    draggable={false}
                />
                <img
                    src={squiggly1Url}
                    alt=""
                    aria-hidden="true"
                    className={`absolute -top-[8vw] left-1/2 w-[80vw] max-w-[1000px] -translate-x-1/2 select-none ${backgroundOpacityClass}`}
                    draggable={false}
                />
                <img
                    src={glitchUrl}
                    alt=""
                    aria-hidden="true"
                    className={`absolute -right-[12vw] top-[70vh] w-[60vw] max-w-[720px] select-none ${backgroundOpacityClass}`}
                    draggable={false}
                />
            </div>

            {content}
        </div>
    );
};
