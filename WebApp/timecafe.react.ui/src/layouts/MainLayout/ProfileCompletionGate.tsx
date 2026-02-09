import type {FC} from "react";
import {useCallback, useEffect, useMemo, useRef, useState} from "react";
import {useDispatch, useSelector} from "react-redux";
import type {AppDispatch, RootState} from "@store";
import {useNavigate} from "react-router-dom";
import {
    Body1,
    Button,
    Caption1,
    Dialog,
    DialogActions,
    DialogBody,
    DialogContent,
    DialogSurface,
    DialogTitle,
    Field,
    Input,
    Radio,
    RadioGroup,
    Spinner,
} from "@fluentui/react-components";
import {fetchProfileByUserId, resetProfile, updateProfile} from "@store/profileSlice";
import {isNameCompleted} from "@utility/profileCompletion";
import {getJwtInfo} from "@shared/auth/jwt";
import {
    clearTokens,
    setEmail,
    setPhoneNumber,
    setPhoneNumberConfirmed,
    setRole,
    setUserId
} from "@store/authSlice";
import {DateInput, PhoneInput} from "@components/FormFields";
import {Gender} from "@app-types/profile";
import {PhoneVerificationModal} from "@components/PhoneVerificationModal/PhoneVerificationModal";
import {
    isPhoneVerificationSessionV1,
    PHONE_VERIFICATION_SESSION_KEY,
    type PhoneVerificationSessionV1,
} from "@shared/auth/phoneVerificationSession";
import {useLocalStorageJson} from "@hooks/useLocalStorageJson";
import {validatePhoneNumber} from "@utility/validate";
import {getUserMessageFromUnknown} from "@api/errors/getUserMessageFromUnknown";
import {authApi} from "@api/auth/authApi";
import {normalizeDate} from "@utility/normalizeDate";

export const ProfileCompletionGate: FC = () => {
    const dispatch = useDispatch<AppDispatch>();
    const navigate = useNavigate();

    const handleLogout = () => {
        dispatch(clearTokens() as never);
        navigate("/login", {replace: true});
    };

    const accessToken = useSelector((state: RootState) => state.auth.accessToken);
    const userId = useSelector((state: RootState) => state.auth.userId);

    const profile = useSelector((state: RootState) => state.profile.data);
    const profileLoading = useSelector((state: RootState) => state.profile.loading);
    const profileSaving = useSelector((state: RootState) => state.profile.saving);
    const profileError = useSelector((state: RootState) => state.profile.error);
    const profileLoadedUserId = useSelector((state: RootState) => state.profile.loadedUserId);
    const authPhoneNumber = useSelector((state: RootState) => state.auth.phoneNumber);
    const authPhoneConfirmed = useSelector((state: RootState) => state.auth.phoneNumberConfirmed);

    const {load: loadPhoneSession} = useLocalStorageJson<PhoneVerificationSessionV1>(
        PHONE_VERIFICATION_SESSION_KEY,
        isPhoneVerificationSessionV1
    );

    const requestedProfileUserIdRef = useRef<string | null>(null);

    const derivedAuthInfo = useMemo(() => {
        if (!accessToken) return null;
        return getJwtInfo(accessToken);
    }, [accessToken]);

    const effectiveUserId = useMemo(() => {
        const trimmed = (userId ?? "").trim();
        if (trimmed) return trimmed;
        const fromToken = derivedAuthInfo?.userId?.trim();
        return fromToken ? fromToken : "";
    }, [derivedAuthInfo?.userId, userId]);

    useEffect(() => {
        if (!accessToken) return;
        if (userId) return;

        if (derivedAuthInfo?.userId) dispatch(setUserId(derivedAuthInfo.userId) as never);
        if (derivedAuthInfo?.role) dispatch(setRole(derivedAuthInfo.role) as never);
        if (derivedAuthInfo?.email) dispatch(setEmail(derivedAuthInfo.email) as never);
    }, [accessToken, derivedAuthInfo, dispatch, userId]);


    useEffect(() => {
        if (!accessToken) {
            requestedProfileUserIdRef.current = null;
            void dispatch(resetProfile() as never);
            return;
        }

        if (!effectiveUserId) {
            requestedProfileUserIdRef.current = null;
            return;
        }

        if (profile && !profileLoadedUserId) {
            requestedProfileUserIdRef.current = null;
            void dispatch(resetProfile() as never);
            return;
        }

        if (profileLoadedUserId && profileLoadedUserId !== effectiveUserId) {
            requestedProfileUserIdRef.current = null;
            void dispatch(resetProfile() as never);
        }

        if (requestedProfileUserIdRef.current !== effectiveUserId) {
            requestedProfileUserIdRef.current = null;
        }
    }, [accessToken, dispatch, effectiveUserId, profile, profileLoadedUserId]);

    useEffect(() => {
        if (!effectiveUserId) return;
        if (profileLoading) return;
        if (profileError) return;
        if (requestedProfileUserIdRef.current === effectiveUserId) return;

        requestedProfileUserIdRef.current = effectiveUserId;
        void dispatch(fetchProfileByUserId({userId: effectiveUserId}) as never);
    }, [dispatch, effectiveUserId, profileError, profileLoading]);

    const [loadingTimedOut, setLoadingTimedOut] = useState(false);
    useEffect(() => {
        if (!profileLoading) {
            setLoadingTimedOut(false);
            return;
        }

        const raw = import.meta.env.VITE_HTTP_TIMEOUT_MS as string | undefined;
        const parsed = raw ? Number(raw) : NaN;
        const timeoutMs = Number.isFinite(parsed) && parsed > 0 ? parsed : 15000;
        const timer = window.setTimeout(() => setLoadingTimedOut(true), timeoutMs + 1500);
        return () => window.clearTimeout(timer);
    }, [profileLoading]);

    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [middleName, setMiddleName] = useState("");
    const [birthDate, setBirthDate] = useState<Date | undefined>(undefined);
    const [genderId, setGenderId] = useState<0 | 1 | 2>(Gender.NotSpecified);
    const [phoneDraft, setPhoneDraft] = useState("");
    const [showPhoneModal, setShowPhoneModal] = useState(false);
    const [phoneAutoSend, setPhoneAutoSend] = useState(false);
    const [phoneSendError, setPhoneSendError] = useState<string | null>(null);
    const [saveError, setSaveError] = useState<string | null>(null);
    const phoneVerifiedRecentlyRef = useRef<{ phone: string; at: number } | null>(null);

    useEffect(() => {
        if (!accessToken) return;
        const session = loadPhoneSession();
        if (!session?.open) return;
        if (showPhoneModal) return;
        const nextPhone = session.phoneNumber.trim();
        if (nextPhone) setPhoneDraft((prev) => (prev.trim() ? prev : nextPhone));
        setPhoneAutoSend(false);
        setShowPhoneModal(true);
    }, [accessToken, loadPhoneSession, showPhoneModal]);

    const isProfileForCurrentUser = useMemo(() => {
        if (!profile) return false;
        if (!effectiveUserId) return false;
        return profileLoadedUserId === effectiveUserId;
    }, [effectiveUserId, profile, profileLoadedUserId]);

    const gateDecisionReady = useMemo(() => {
        if (!accessToken) return false;
        if (!effectiveUserId) return true;
        if (profileLoading) return false;
        if (profileError && !profile) return true;
        return isProfileForCurrentUser;
    }, [accessToken, effectiveUserId, isProfileForCurrentUser, profile, profileError, profileLoading]);

    const mustCompleteProfile = useMemo(() => {
        if (!accessToken) return false;
        if (!gateDecisionReady) return false;
        if (!effectiveUserId) return true;
        if (profileError && !profile) return true;
        if (!isProfileForCurrentUser) return false;
        if (!profile) return false;
        if (!isNameCompleted(profile)) return true;

        const trimmedPhone = phoneDraft.trim();
        return !!(trimmedPhone && phoneSendError);


    }, [accessToken, effectiveUserId, gateDecisionReady, isProfileForCurrentUser, phoneDraft, phoneSendError, profile, profileError]);

    const toDateOnlyStringOrUndefined = (value: Date | undefined): string | undefined => {
        if (!value) return undefined;
        const yyyy = value.getFullYear();
        const mm = String(value.getMonth() + 1).padStart(2, "0");
        const dd = String(value.getDate()).padStart(2, "0");
        return `${yyyy}-${mm}-${dd}`;
    };

    useEffect(() => {
        if (!profile) return;
        setFirstName(profile.firstName ?? "");
        setLastName(profile.lastName ?? "");
        setMiddleName(profile.middleName ?? "");
        setBirthDate(normalizeDate(profile.birthDate));
        setPhoneDraft((prev) => {
            const profilePhone = (profile.phoneNumber ?? "").trim();
            if (profilePhone) return profilePhone;
            const current = prev.trim();
            if (current) return prev;
            const authPhone = (authPhoneNumber ?? "").trim();
            return authPhone || prev;
        });
        setSaveError(null);
        setPhoneSendError(null);
        const g = profile.gender;
        setGenderId(g === Gender.Male || g === Gender.Female ? g : Gender.NotSpecified);
    }, [authPhoneNumber, profile?.firstName, profile?.lastName, profile?.middleName, profile]);

    useEffect(() => {
        const authPhone = (authPhoneNumber ?? "").trim();
        if (!authPhone) return;
        setPhoneDraft((prev) => (prev.trim() ? prev : authPhone));
    }, [authPhoneNumber]);

    const canSave = Boolean(firstName.trim()) && Boolean(lastName.trim()) && !profileSaving && !profileLoading;

    const retryLoadProfile = useCallback(() => {
        setLoadingTimedOut(false);
        requestedProfileUserIdRef.current = null;
        void dispatch(resetProfile() as never);
        void dispatch(fetchProfileByUserId({userId: String(effectiveUserId)}) as never);
    }, [dispatch, effectiveUserId]);

    const renderDialogContent = () => {
        if (!effectiveUserId) {
            return (
                <div className="flex flex-col gap-3">
                    <Body1>Не удалось определить пользователя из сессии.</Body1>
                    <Caption1>Попробуйте перезайти в систему.</Caption1>

                    <Button appearance="primary" onClick={handleLogout}>
                        На страницу входа
                    </Button>
                </div>
            );
        }

        if (loadingTimedOut) {
            return (
                <div className="flex flex-col gap-3">
                    <Body1>Загрузка профиля заняла слишком много времени.</Body1>
                    <Caption1>Проверьте доступность API и повторите попытку.</Caption1>

                    <Button appearance="primary" onClick={retryLoadProfile}>
                        Повторить
                    </Button>
                </div>
            );
        }

        if (profileLoading) {
            return (
                <div className="flex items-center gap-3">
                    <Spinner size="small"/>
                    <Body1>Загружаем профиль…</Body1>
                </div>
            );
        }

        if (!profile && profileError) {
            return (
                <div className="flex flex-col gap-3">
                    <Body1>Не удалось загрузить профиль. Повторите попытку позже.</Body1>
                    <Caption1>{profileError}</Caption1>

                    <Button appearance="primary" onClick={retryLoadProfile}>
                        Повторить
                    </Button>
                </div>
            );
        }

        if (!profile) {
            return (
                <div className="flex items-center gap-3">
                    <Spinner size="small"/>
                    <Body1>Подготавливаем профиль…</Body1>
                </div>
            );
        }

        return (
            <div className="flex flex-col gap-3">
                <Body1>
                    Это обязательный шаг. Для сохранения нужны обязательные поля (имя и фамилия), остальные данные
                    желательно заполнить сейчас.
                </Body1>

                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                    <Field label="Фамилия" required>
                        <Input value={lastName} onChange={(_, d) => setLastName(d.value)}/>
                    </Field>
                    <Field label="Имя" required>
                        <Input value={firstName} onChange={(_, d) => setFirstName(d.value)}/>
                    </Field>
                    <Field label="Отчество">
                        <Input value={middleName} onChange={(_, d) => setMiddleName(d.value)}/>
                    </Field>

                    <DateInput
                        value={birthDate}
                        onChange={setBirthDate}
                        disabled={profileSaving || profileLoading}
                        label="Дата рождения"
                        required={false}
                        maxDate={new Date()}
                    />
                </div>

                <Field label="Пол">
                    <RadioGroup
                        value={String(genderId)}
                        disabled={profileSaving || profileLoading}
                        onChange={(_, data) => {
                            const next = Number.parseInt(data.value, 10);
                            setGenderId(next === 1 || next === 2 ? (next as 1 | 2) : 0);
                        }}
                    >
                        <Radio value="0" label="Не указан"/>
                        <Radio value="1" label="Мужчина"/>
                        <Radio value="2" label="Женщина"/>
                    </RadioGroup>
                </Field>

                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                    <Field label="Email">
                        <Input value={(profile.email ?? derivedAuthInfo?.email ?? "").trim()} disabled/>
                    </Field>

                    <PhoneInput
                        value={phoneDraft}
                        onChange={(value) => {
                            setPhoneDraft(value);
                            setSaveError(null);
                        }}
                        disabled={profileSaving || profileLoading}
                        label="Телефон"
                        required={false}
                        validateOnBlur
                        onValidationChange={(err) => setPhoneSendError(err ? err : null)}
                    />
                </div>

                {saveError ? <Caption1 className="text-red-600">{saveError}</Caption1> : null}
            </div>
        );
    };

    return (
        <>
            <Dialog open={mustCompleteProfile} modalType="alert" unmountOnClose={false}>
                <DialogSurface>
                    <DialogBody>
                        <DialogTitle>Заполните профиль</DialogTitle>

                        <DialogContent>
                            {renderDialogContent()}
                        </DialogContent>

                        <DialogActions>
                            <Button appearance="secondary" onClick={handleLogout}>
                                Выйти
                            </Button>
                            <Button
                                appearance="primary"
                                disabled={!canSave}
                                onClick={async () => {
                                    setPhoneSendError(null);
                                    setSaveError(null);

                                    try {
                                        await dispatch(
                                            updateProfile({
                                                firstName: firstName.trim(),
                                                lastName: lastName.trim(),
                                                middleName: middleName.trim() || undefined,
                                                birthDate: toDateOnlyStringOrUndefined(birthDate),
                                                gender: genderId,
                                            })
                                        ).unwrap();

                                        requestedProfileUserIdRef.current = null;
                                        void dispatch(fetchProfileByUserId({userId: String(effectiveUserId)}));

                                        const nextPhone = phoneDraft.trim();
                                        const currentPhone = (authPhoneNumber ?? profile?.phoneNumber ?? "").trim();
                                        const phoneNeedsVerification = Boolean(nextPhone) && (nextPhone !== currentPhone || !authPhoneConfirmed);

                                        if (phoneNeedsVerification) {
                                            const validation = validatePhoneNumber(nextPhone);
                                            if (validation) {
                                                setPhoneSendError(validation);
                                                return;
                                            }

                                            const recently = phoneVerifiedRecentlyRef.current;
                                            if (recently && recently.phone === nextPhone && Date.now() - recently.at < 30_000) return;

                                            if (showPhoneModal) return;

                                            setPhoneAutoSend(true);
                                            setShowPhoneModal(true);
                                        }
                                    } catch (err: unknown) {
                                        setSaveError(getUserMessageFromUnknown(err) || "Не удалось сохранить профиль.");
                                    }
                                }}
                            >
                                Сохранить
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>

            <PhoneVerificationModal
                isOpen={showPhoneModal}
                onClose={() => {
                    setShowPhoneModal(false);
                    setPhoneAutoSend(false);
                }}
                currentPhoneNumber={phoneDraft.trim()}
                currentPhoneNumberConfirmed={authPhoneConfirmed}
                onPhoneNumberSaved={(nextPhone) => {
                    dispatch(setPhoneNumber(nextPhone) as never);
                    dispatch(setPhoneNumberConfirmed(false) as never);
                }}
                autoSendCodeOnOpen={phoneAutoSend}
                onSuccess={async (verifiedPhone: string) => {
                    setShowPhoneModal(false);
                    setPhoneAutoSend(false);
                    setPhoneDraft(verifiedPhone);
                    phoneVerifiedRecentlyRef.current = {phone: verifiedPhone, at: Date.now()};
                    try {
                        const currentUser = await authApi.getCurrentUser();
                        dispatch(setPhoneNumber(currentUser.phoneNumber ?? "") as never);
                        dispatch(setPhoneNumberConfirmed(currentUser.phoneNumberConfirmed) as never);
                    } catch {
                        void 0;
                    }
                    requestedProfileUserIdRef.current = null;
                    void dispatch(resetProfile() as never);
                    void dispatch(fetchProfileByUserId({userId: String(effectiveUserId)}) as never);
                }}
            />
        </>
    );
};
