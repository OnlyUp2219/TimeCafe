import type {FC} from "react";
import {useCallback, useEffect, useMemo, useRef, useState} from "react";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {useNavigate} from "react-router-dom";
import {
    Button,
    Card,
    DrawerBody,
    DrawerFooter,
    DrawerHeader,
    DrawerHeaderTitle,
    OverlayDrawer,
} from "@fluentui/react-components";
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
import {Gender} from "@app-types/profile";
import {PhoneVerificationModal} from "@components/PhoneVerificationModal/PhoneVerificationModal";
import {validatePhoneNumber} from "@utility/validate";
import {getUserMessageFromUnknown} from "@api/errors/getUserMessageFromUnknown";
import {useLazyGetCurrentUserQuery} from "@store/api/authApi";
import {useGetProfileByUserIdQuery, useUpdateProfileMutation} from "@store/api/profileApi";
import {normalizeDate} from "@utility/normalizeDate";
import {GateStatusContent} from "./GateStatusContent";
import {ProfileGateForm} from "./ProfileGateForm";

export const ProfileCompletionGate: FC = () => {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const [getCurrentUser] = useLazyGetCurrentUserQuery();

    const handleLogout = () => {
        dispatch(clearTokens() as never);
        navigate("/login", {replace: true});
    };

    const accessToken = useAppSelector((state) => state.auth.accessToken);
    const userId = useAppSelector((state) => state.auth.userId);

    const authPhoneNumber = useAppSelector((state) => state.auth.phoneNumber);
    const authPhoneConfirmed = useAppSelector((state) => state.auth.phoneNumberConfirmed);

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

    const {
        data: profile,
        isLoading: profileLoading,
        error: profileQueryError,
        isFetching: profileFetching,
        refetch: refetchProfile,
    } = useGetProfileByUserIdQuery(effectiveUserId, {skip: !effectiveUserId});

    const profileError = profileQueryError ? getUserMessageFromUnknown(profileQueryError) : null;
    const [updateProfileMutation, {isLoading: profileSaving}] = useUpdateProfileMutation();

    const [loadingTimedOut, setLoadingTimedOut] = useState(false);
    useEffect(() => {
        if (!profileFetching) {
            setLoadingTimedOut(false);
            return;
        }

        const raw = import.meta.env.VITE_HTTP_TIMEOUT_MS as string | undefined;
        const parsed = raw ? Number(raw) : NaN;
        const timeoutMs = Number.isFinite(parsed) && parsed > 0 ? parsed : 15000;
        const timer = window.setTimeout(() => setLoadingTimedOut(true), timeoutMs + 1500);
        return () => window.clearTimeout(timer);
    }, [profileFetching]);

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

    const gateDecisionReady = useMemo(() => {
        if (!accessToken) return false;
        if (!effectiveUserId) return true;
        if (profileLoading) return false;
        if (profileError && !profile) return true;
        return !!profile;
    }, [accessToken, effectiveUserId, profile, profileError, profileLoading]);

    const mustCompleteProfile = useMemo(() => {
        if (!accessToken) return false;
        if (!gateDecisionReady) return false;
        if (!effectiveUserId) return true;
        if (profileError && !profile) return true;
        if (!profile) return false;
        if (!isNameCompleted(profile)) return true;

        const trimmedPhone = phoneDraft.trim();
        return !!(trimmedPhone && phoneSendError);


    }, [accessToken, effectiveUserId, gateDecisionReady, phoneDraft, phoneSendError, profile, profileError]);

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
        refetchProfile();
    }, [refetchProfile]);

    const renderGateContent = () => {
        if (!effectiveUserId || loadingTimedOut || profileLoading || (!profile && profileError) || !profile) {
            return (
                <GateStatusContent
                    effectiveUserId={effectiveUserId}
                    profileLoading={profileLoading}
                    profileError={profileError}
                    loadingTimedOut={loadingTimedOut}
                    hasProfile={!!profile}
                    onLogout={handleLogout}
                    onRetry={retryLoadProfile}
                />
            );
        }

        return (
            <ProfileGateForm
                profile={profile}
                firstName={firstName}
                lastName={lastName}
                middleName={middleName}
                birthDate={birthDate}
                genderId={genderId}
                phoneDraft={phoneDraft}
                email={(profile.email ?? derivedAuthInfo?.email ?? "").trim()}
                saveError={saveError}
                disabled={profileSaving || profileLoading}
                onFirstNameChange={setFirstName}
                onLastNameChange={setLastName}
                onMiddleNameChange={setMiddleName}
                onBirthDateChange={setBirthDate}
                onGenderChange={setGenderId}
                onPhoneChange={setPhoneDraft}
                onPhoneValidationChange={(err) => setPhoneSendError(err)}
                onSaveErrorClear={() => setSaveError(null)}
            />
        );
    };

    return (
        <>
            <OverlayDrawer
                open={mustCompleteProfile}
                modalType="alert"
                size="full"
                position="bottom"

            >
                <div className="profile-gate-shell">
                    <div className="profile-gate-bg" aria-hidden="true">
                        <span className="profile-gate-shape profile-gate-shape--circle"/>
                        <span className="profile-gate-shape profile-gate-shape--square"/>
                        <span className="profile-gate-shape profile-gate-shape--triangle"/>
                    </div>

                    <DrawerHeader >
                        <DrawerHeaderTitle data-testid="profile-gate-title">Заполните профиль</DrawerHeaderTitle>
                    </DrawerHeader>

                    <DrawerBody className="profile-gate-body">
                        <Card size="large" >
                            {renderGateContent()}
                        </Card>
                    </DrawerBody>

                    <DrawerFooter className="profile-gate-footer" >
                        <Button appearance="secondary" onClick={handleLogout}>
                            Выйти
                        </Button>
                        <Button
                            appearance="primary"
                            data-testid="profile-gate-save"
                            disabled={!canSave}
                            onClick={async () => {
                                setPhoneSendError(null);
                                setSaveError(null);

                                try {
                                    await updateProfileMutation({
                                        userId: effectiveUserId,
                                        firstName: firstName.trim(),
                                        lastName: lastName.trim(),
                                        middleName: middleName.trim() || null,
                                        photoUrl: profile?.photoUrl ?? null,
                                        birthDate: toDateOnlyStringOrUndefined(birthDate) ?? null,
                                        gender: genderId,
                                    }).unwrap();

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
                    </DrawerFooter>
                </div>
            </OverlayDrawer>

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
                        const currentUser = await getCurrentUser().unwrap();
                        dispatch(setPhoneNumber(currentUser.phoneNumber ?? "") as never);
                        dispatch(setPhoneNumberConfirmed(currentUser.phoneNumberConfirmed) as never);
                    } catch {
                        void 0;
                    }
                    refetchProfile();
                }}
            />
        </>
    );
};
