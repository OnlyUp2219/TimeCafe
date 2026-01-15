import {
    Avatar,
    Badge,
    Body2,
    Button,
    Divider,
    Tag,
    Title2,
    tokens,
} from "@fluentui/react-components";
import {useMemo, useCallback} from "react";
import {useDispatch, useSelector} from "react-redux";
import {useNavigate} from "react-router-dom";
import type {RootState} from "../../store";
import {PersonalInfoForm} from "../../components/PersonalDataForm/PersonalInfoForm";
import {ChangePasswordForm} from "../../components/PersonalDataForm/ChangePasswordForm";
import type {ClientInfo} from "../../types/client";
import {setClient, updateClientProfile} from "../../store/clientSlice";
import type {AppDispatch} from "../../store";
import {useProgressToast} from "../../components/ToastProgress/ToastProgress";

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

    const subtleTextStyle = useMemo(() => ({color: tokens.colorNeutralForeground2}), []);

    const handleSave = useCallback(
        async (next: ClientInfo) => {
            const patch: Partial<ClientInfo> = {
                email: next.email,
                phoneNumber: next.phoneNumber,
                birthDate: next.birthDate,
                genderId: next.genderId,
            };

            const action = await dispatch(updateClientProfile(patch));
            if (updateClientProfile.fulfilled.match(action)) {
                showToast("Данные профиля сохранены.", "success", "Готово");
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

    const content = !client ? (
        <div className="mx-auto w-full max-w-3xl px-4 py-6 relative z-10">
            <div
                className="rounded-3xl p-6"
                style={{
                    backgroundImage: `radial-gradient(900px 480px at 20% 10%, ${tokens.colorBrandBackground2} 0%, transparent 60%), radial-gradient(720px 420px at 90% 0%, ${tokens.colorPaletteLightGreenBackground2} 0%, transparent 55%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
                    border: `1px solid ${tokens.colorNeutralStroke1}`,
                    boxShadow: tokens.shadow16,
                }}
            >
                <Title2>Персональные данные</Title2>
                <div className="mt-2" style={subtleTextStyle}>
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
        <div className="mx-auto w-full max-w-6xl px-4 py-6 relative z-10">
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
                                image={client.photo ? {src: client.photo} : undefined}
                                size={56}
                            />

                            <div className="flex flex-col gap-1 min-w-0">
                                <div className="flex flex-wrap items-center gap-2">
                                    <Title2>Профиль</Title2>
                                    <Badge appearance="tint" size="large">PersonalData</Badge>
                                </div>
                                <Body2 style={subtleTextStyle}>
                                    Обновляйте контакты и основные данные — они используются для уведомлений и восстановления доступа.
                                </Body2>
                            </div>
                        </div>

                        <div className="flex flex-wrap gap-2">
                            <Tag appearance={client.emailConfirmed ? "brand" : "outline"}>
                                {client.emailConfirmed ? "Email подтверждён" : "Email не подтверждён"}
                            </Tag>
                            <Tag appearance={client.phoneNumberConfirmed ? "brand" : "outline"}>
                                {client.phoneNumberConfirmed ? "Телефон подтверждён" : "Телефон не подтверждён"}
                            </Tag>
                        </div>
                    </div>

                    <Divider/>

                    {saveError && (
                        <div style={subtleTextStyle}>
                            Последняя ошибка сохранения: {saveError}
                        </div>
                    )}

                    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                        <PersonalInfoForm
                            client={client}
                            className="h-full"
                            loading={saving}
                            showDownloadButton={false}
                            onSave={handleSave}
                        />
                        <ChangePasswordForm
                            className="h-full"
                            redirectToLoginOnSuccess
                            autoClearTokensOnSuccess
                        />
                    </div>
                </div>
            </div>
        </div>
    );

    return (
        <div className="tc-noise-overlay relative overflow-hidden  h-full">
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
