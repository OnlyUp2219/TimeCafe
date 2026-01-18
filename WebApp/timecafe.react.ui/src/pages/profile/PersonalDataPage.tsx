import {
    Avatar,
    Badge,
    Body2,
    Button,
    Card,
    Divider,
    Title2,
    tokens,
} from "@fluentui/react-components";
import {useCallback} from "react";
import {useState} from "react";
import {useDispatch, useSelector} from "react-redux";
import {useNavigate} from "react-router-dom";
import type {RootState} from "../../store";
import {ChangePasswordForm} from "../../components/PersonalDataForm/ChangePasswordForm";
import type {ClientInfo} from "../../types/client";
import {setClient, updateClientProfile} from "../../store/clientSlice";
import type {AppDispatch} from "../../store";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress";
import {PersonalDataMainForm} from "../../components/PersonalDataForm/PersonalDataMainForm";
import {PhoneFormCard} from "../../components/PersonalDataForm/PhoneFormCard";
import {EmailFormCard} from "../../components/PersonalDataForm/EmailFormCard";
import {LogoutCard} from "../../components/PersonalDataForm/LogoutCard";
import {logoutServer} from "../../api/auth.ts";
import {PasswordFilled} from "@fluentui/react-icons";
import blob3Url from "../../assets/ssshape_blob3.svg";
import blob4Url from "../../assets/ssshape_blob4.svg";
import squiggly1Url from "../../assets/sssquiggl_1.svg";
import glitchUrl from "../../assets/ggglitch.svg";
import "./PersonalData.css";

export const PersonalDataPage = () => {
    const navigate = useNavigate();
    const dispatch = useDispatch<AppDispatch>();
    const client = useSelector((state: RootState) => state.client.data);
    const saving = useSelector((state: RootState) => state.client.saving);
    const saveError = useSelector((state: RootState) => state.client.error);

    const {showToast, ToasterElement} = useProgressToast();

    const savePatch = useCallback(
        async (patch: Partial<ClientInfo>, successMessage: string) => {
            const action = await dispatch(updateClientProfile(patch));
            if (updateClientProfile.fulfilled.match(action)) {
                showToast(successMessage, "success", "Готово");
                return;
            }

            const message =
                (updateClientProfile.rejected.match(action) && (action.payload as string | undefined)) ||
                "Не удалось сохранить профиль.";
            showToast(message, "error", "Ошибка");
        },
        [dispatch, showToast]
    );

    const loadDemoProfile = useCallback(() => {
        const demo: ClientInfo = {
            clientId: 1,
            firstName: "Иван",
            lastName: "Иванов",
            middleName: "Иванович",
            email: "demo@timecafe.local",
            emailConfirmed: true,
            phoneNumber: "+375291234567",
            phoneNumberConfirmed: true,
            birthDate: new Date("1999-01-15"),
            genderId: 1,
            accessCardNumber: "TC-000001",
        };

        dispatch(setClient(demo));
        showToast("Загружен демо-профиль для теста UI.", "info", "Demo");
    }, [dispatch, showToast]);

    const backgroundOpacity = client ? 0.08 : 0.10;

    const [photoUrl, setPhotoUrl] = useState<string | null | undefined>(undefined);
    const [showPasswordForm, setShowPasswordForm] = useState(false);

    const handleLogout = useCallback(async () => {
        await logoutServer(dispatch);
        navigate("/login", {replace: true});
    }, [dispatch, navigate]);

    const content = !client ? (
        <div className="mx-auto w-full max-w-3xl px-2 py-4 sm:px-3 sm:py-6 relative z-10">
            <div
                className="rounded-3xl p-6"
                style={{
                    backgroundImage: `radial-gradient(900px 480px at 20% 10%, ${tokens.colorBrandBackground2} 0%, transparent 60%), radial-gradient(720px 420px at 90% 0%, ${tokens.colorPaletteLightGreenBackground2} 0%, transparent 55%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
                    border: `1px solid ${tokens.colorNeutralStroke1}`,
                    boxShadow: tokens.shadow16,
                }}
            >
                <Title2>Персональные данные</Title2>
                <div>
                    Профиль не загружен. Перейдите на главную и попробуйте снова.
                </div>

                <div className="mt-4 flex gap-2">
                    <Button appearance="primary" onClick={() => navigate("/home")}>
                        Вернуться на главную
                    </Button>
                    <Button appearance="secondary" onClick={loadDemoProfile}>
                        Загрузить демо-профиль
                    </Button>
                </div>
            </div>
        </div>
    ) : (
        <div className="mx-auto  w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6 relative z-10 ">
            <div
                className="rounded-3xl p-5 sm:p-8"
                style={{
                    backgroundImage: `radial-gradient(900px 480px at 20% 10%, ${tokens.colorBrandBackground2} 0%, transparent 60%), radial-gradient(720px 420px at 90% 0%, ${tokens.colorPaletteLightGreenBackground2} 0%, transparent 55%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
                    border: `1px solid ${tokens.colorNeutralStroke1}`,
                    boxShadow: tokens.shadow16,
                }}
            >
                <div className="flex flex-col gap-4">
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                        <div className="flex items-center gap-3 min-w-0">
                            <Avatar
                                name={`${client.lastName} ${client.firstName}${client.middleName ? ` ${client.middleName}` : ""}`.trim() || client.email}
                                color="colorful"
                                initials={`${client.firstName?.[0] ?? ""}${client.lastName?.[0] ?? ""}`.trim() || "TC"}
                                image={
                                    photoUrl !== undefined
                                        ? (photoUrl ? {src: photoUrl} : undefined)
                                        : (client.photo ? {src: client.photo} : undefined)
                                }
                                size={56}
                            />

                            <div className="flex flex-col gap-1 min-w-0">
                                <div className="flex flex-wrap items-center gap-2">
                                    <Title2>Профиль</Title2>
                                    <Badge appearance="tint" size="large">PersonalData</Badge>
                                </div>
                                <Body2>
                                    Обновляйте контакты и основные данные — они используются для уведомлений и
                                    восстановления доступа.
                                </Body2>
                            </div>
                        </div>

                        <div/>
                    </div>

                    <Divider/>

                    {saveError && (
                        <div>
                            Последняя ошибка сохранения: {saveError}
                        </div>
                    )}

                    <div className="flex flex-col gap-4">
                        <PersonalDataMainForm
                            client={client}
                            loading={saving}
                            onPhotoUrlChange={(url) => setPhotoUrl(url)}
                            onSave={(patch) => savePatch(patch, "Персональные данные сохранены.")}
                        />

                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-2 ">
                            <PhoneFormCard
                                client={client}
                                loading={saving}
                                onSave={(patch) => savePatch(patch, "Телефон сохранён.")}
                            />

                            <EmailFormCard
                                client={client}
                                loading={saving}
                                onSave={(patch) => savePatch(patch, "Почта сохранена.")}
                            />

                            <Card className="sm:col-span-2 lg:col-span-1">
                                <Title2>Смена пароля</Title2>
                                <Body2 className="!line-clamp-2">
                                    Рекомендуется менять пароль регулярно и использовать уникальные пароли для
                                    разных сервисов.
                                </Body2>
                                <div
                                    className={!showPasswordForm ? "flex flex-col sm:flex-row sm:items-center" : "w-full"}>
                                    {!showPasswordForm ? (
                                        <Button appearance="primary" icon={<PasswordFilled/>}
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

                            <LogoutCard className="sm:col-span-2 lg:col-span-1" onLogout={handleLogout}/>
                        </div>

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
                    className="absolute -top-[10vw] -right-[10vw] w-[50vw] max-w-[640px] rotate-6 select-none"
                    style={{opacity: backgroundOpacity}}
                    draggable={false}
                />
                <img
                    src={blob4Url}
                    alt=""
                    aria-hidden="true"
                    className="absolute -left-[12vw] top-[35vh] w-[55vw] max-w-[720px] -rotate-6 select-none"
                    style={{opacity: backgroundOpacity}}
                    draggable={false}
                />
                <img
                    src={squiggly1Url}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[8vw] left-1/2 w-[80vw] max-w-[1000px] -translate-x-1/2 select-none"
                    style={{opacity: backgroundOpacity}}
                    draggable={false}
                />
                <img
                    src={glitchUrl}
                    alt=""
                    aria-hidden="true"
                    className="absolute -right-[12vw] top-[70vh] w-[60vw] max-w-[720px] select-none"
                    style={{opacity: backgroundOpacity}}
                    draggable={false}
                />
            </div>

            {content}
        </div>
    );
};
