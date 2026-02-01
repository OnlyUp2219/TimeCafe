import type {FC} from "react";
import {useEffect, useMemo, useRef, useState} from "react";
import {useDispatch, useSelector} from "react-redux";
import type {RootState} from "../../store";
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
    Spinner,
} from "@fluentui/react-components";
import {fetchProfileByUserId, resetProfile, updateProfile} from "../../store/profileSlice";
import {isNameCompleted} from "../../utility/profileCompletion";
import {getJwtInfo} from "../../shared/auth/jwt";
import {clearTokens, setEmail, setEmailConfirmed, setRole, setUserId} from "../../store/authSlice";

export const ProfileCompletionGate: FC = () => {
    const dispatch = useDispatch();
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

        if (derivedAuthInfo) {
            dispatch(setEmailConfirmed(true) as never);
        }
    }, [accessToken, derivedAuthInfo, dispatch, userId]);

    useEffect(() => {
        if (!effectiveUserId) {
            requestedProfileUserIdRef.current = null;
            return;
        }

        if (requestedProfileUserIdRef.current !== effectiveUserId) {
            requestedProfileUserIdRef.current = null;
        }
    }, [effectiveUserId]);

    useEffect(() => {
        if (!effectiveUserId) return;
        if (profile) return;
        if (profileLoading) return;
        if (profileError) return;
        if (requestedProfileUserIdRef.current === effectiveUserId) return;

        requestedProfileUserIdRef.current = effectiveUserId;
        void dispatch(fetchProfileByUserId({userId: effectiveUserId}) as never);
    }, [dispatch, effectiveUserId, profile, profileError, profileLoading]);

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

    const mustCompleteProfile = useMemo(() => {
        if (!accessToken) return false;
        if (!effectiveUserId) return true;
        if (profileLoading) return true;
        if (!profile) return true;
        return !isNameCompleted(profile);
    }, [accessToken, effectiveUserId, profile, profileLoading]);

    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [middleName, setMiddleName] = useState("");

    useEffect(() => {
        if (!profile) return;
        setFirstName(profile.firstName ?? "");
        setLastName(profile.lastName ?? "");
        setMiddleName(profile.middleName ?? "");
    }, [profile?.firstName, profile?.lastName, profile?.middleName, profile]);

    const canSave = Boolean(firstName.trim()) && Boolean(lastName.trim()) && !profileSaving && !profileLoading;

    return (
        <Dialog open={mustCompleteProfile} modalType="alert">
            <DialogSurface>
                <DialogBody>
                    <DialogTitle>Заполните профиль</DialogTitle>

                    <DialogContent>
                        {!effectiveUserId ? (
                            <div className="flex flex-col gap-3">
                                <Body1>Не удалось определить пользователя из сессии.</Body1>
                                <Caption1>Попробуйте перезайти в систему.</Caption1>

                                <Button
                                    appearance="primary"
                                    onClick={handleLogout}
                                >
                                    На страницу входа
                                </Button>
                            </div>
                        ) : loadingTimedOut ? (
                            <div className="flex flex-col gap-3">
                                <Body1>Загрузка профиля заняла слишком много времени.</Body1>
                                <Caption1>Проверьте доступность API и повторите попытку.</Caption1>

                                <Button
                                    appearance="primary"
                                    onClick={() => {
                                        setLoadingTimedOut(false);
                                        requestedProfileUserIdRef.current = null;
                                        void dispatch(resetProfile() as never);
                                        void dispatch(fetchProfileByUserId({userId: String(effectiveUserId)}) as never);
                                    }}
                                >
                                    Повторить
                                </Button>
                            </div>
                        ) : profileLoading ? (
                            <div className="flex items-center gap-3">
                                <Spinner size="small" />
                                <Body1>Загружаем профиль…</Body1>
                            </div>
                        ) : !profile && profileError ? (
                            <div className="flex flex-col gap-3">
                                <Body1>
                                    Не удалось загрузить профиль. Повторите попытку позже.
                                </Body1>
                                <Caption1>{profileError}</Caption1>

                                <Button
                                    appearance="primary"
                                    onClick={() => {
                                        requestedProfileUserIdRef.current = null;
                                        void dispatch(resetProfile() as never);
                                        void dispatch(fetchProfileByUserId({userId: String(effectiveUserId)}) as never);
                                    }}
                                >
                                    Повторить
                                </Button>
                            </div>
                        ) : !profile ? (
                            <div className="flex items-center gap-3">
                                <Spinner size="small" />
                                <Body1>Подготавливаем профиль…</Body1>
                            </div>
                        ) : (
                            <div className="flex flex-col gap-3">
                                <Body1>Это обязательный шаг. Укажите как минимум имя и фамилию.</Body1>

                                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                                    <Field label="Фамилия" required>
                                        <Input value={lastName} onChange={(_, d) => setLastName(d.value)} />
                                    </Field>
                                    <Field label="Имя" required>
                                        <Input value={firstName} onChange={(_, d) => setFirstName(d.value)} />
                                    </Field>
                                    <Field label="Отчество">
                                        <Input value={middleName} onChange={(_, d) => setMiddleName(d.value)} />
                                    </Field>
                                </div>
                            </div>
                        )}
                    </DialogContent>

                    <DialogActions>
                        <Button appearance="secondary" onClick={handleLogout}>
                            Выйти
                        </Button>
                        <Button
                            appearance="primary"
                            disabled={!canSave}
                            onClick={async () => {
                                const action = await dispatch(
                                    updateProfile({
                                        firstName: firstName.trim(),
                                        lastName: lastName.trim(),
                                        middleName: middleName.trim() || undefined,
                                    }) as never
                                );
                                if (updateProfile.fulfilled.match(action)) {
                                    requestedProfileUserIdRef.current = null;
                                    void dispatch(fetchProfileByUserId({userId: String(effectiveUserId)}) as never);
                                }
                            }}
                        >
                            Сохранить
                        </Button>
                    </DialogActions>
                </DialogBody>
            </DialogSurface>
        </Dialog>
    );
};

export default ProfileCompletionGate;
