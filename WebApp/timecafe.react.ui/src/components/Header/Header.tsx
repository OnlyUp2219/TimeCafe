import {Hamburger, Button, Avatar, Tag} from "@fluentui/react-components";
import {useDispatch, useSelector} from "react-redux";
import {useEffect, useMemo, useState, type FC} from "react";
import {useLocation, useNavigate} from "react-router-dom";
import {authApi} from "@api/auth/authApi";
import {clearTokens} from "@store/authSlice";
import type {AppDispatch, RootState} from "@store";
import {formatDurationSeconds} from "@utility/formatDurationSeconds";
import {loadActiveVisitByUser, VisitUiStatus} from "@store/visitSlice";
import {Clock20Regular} from "@fluentui/react-icons";
import {fetchProfileByUserId} from "@store/profileSlice";
import {ProfileApi} from "@api/profile/profileApi";
import {useProgressToast} from "@components/ToastProgress/ToastProgress";

interface HeaderProps {
    onMenuToggle?: () => void;
    isSidebarOpen?: boolean;
    variant?: "app" | "public";
}
// TODO : Refactor - move upload avatar logic to separate hook and reuse in profile page 
export const Header: FC<HeaderProps> = ({onMenuToggle, isSidebarOpen, variant = "app"}) => {
    const dispatch = useDispatch<AppDispatch>();
    const navigate = useNavigate();
    const {showToast} = useProgressToast();
    const location = useLocation();
    const userId = useSelector((state: RootState) => state.auth.userId);
    const authEmail = useSelector((state: RootState) => state.auth.email);
    const profile = useSelector((state: RootState) => state.profile.data);
    const profileLoadedUserId = useSelector((state: RootState) => state.profile.loadedUserId);
    const profileLoading = useSelector((state: RootState) => state.profile.loading);
    const visitStatus = useSelector((state: RootState) => state.visit.status);
    const activeVisit = useSelector((state: RootState) => state.visit.activeVisit);

    const [nowMs, setNowMs] = useState(() => Date.now());
    const [avatarPhotoUrl, setAvatarPhotoUrl] = useState<string | null>(null);

    useEffect(() => {
        if (!userId) return;
        void dispatch(loadActiveVisitByUser({userId}));
    }, [dispatch, userId]);

    useEffect(() => {
        if (!userId) return;
        if (profileLoading) return;
        if (profile && profileLoadedUserId === userId) return;
        void dispatch(fetchProfileByUserId({userId}));
    }, [dispatch, profile, profileLoadedUserId, profileLoading, userId]);

    useEffect(() => {
        if (visitStatus !== VisitUiStatus.Active || !activeVisit) return;
        const timerId = window.setInterval(() => setNowMs(Date.now()), 1000);
        return () => window.clearInterval(timerId);
    }, [visitStatus, activeVisit]);

    const activeVisitDuration = useMemo(() => {
        if (visitStatus !== VisitUiStatus.Active || !activeVisit) return null;
        const elapsedSeconds = Math.max(0, Math.floor((nowMs - activeVisit.startedAtMs) / 1000));
        return formatDurationSeconds(elapsedSeconds);
    }, [activeVisit, nowMs, visitStatus]);

    useEffect(() => {
        let isActive = true;
        let objectUrl: string | null = null;

        const reset = () => {
            setAvatarPhotoUrl(null);
            if (objectUrl) {
                URL.revokeObjectURL(objectUrl);
                objectUrl = null;
            }
        };

        const sourceUrl = profile?.photoUrl?.trim();
        if (!sourceUrl) {
            reset();
            return;
        }

        if (sourceUrl.startsWith("blob:") || sourceUrl.startsWith("data:")) {
            setAvatarPhotoUrl(sourceUrl);
            return;
        }

        const loadPhoto = async () => {
            try {
                const blob = await ProfileApi.getProfilePhotoBlob(sourceUrl);
                if (!isActive) return;
                objectUrl = URL.createObjectURL(blob);
                setAvatarPhotoUrl(objectUrl);
            } catch {
                if (!isActive) return;
                setAvatarPhotoUrl(null);
            }
        };

        void loadPhoto();

        return () => {
            isActive = false;
            if (objectUrl) {
                URL.revokeObjectURL(objectUrl);
            }
        };
    }, [profile?.photoUrl]);

    const handleServerLogout = async () => {
        if (visitStatus === VisitUiStatus.Active) {
            showToast("Сначала завершите активный визит", "warning", "Выход недоступен");
            navigate("/visit/active");
            return;
        }
        await authApi.logout();
        dispatch(clearTokens());
        navigate("/login", { replace: true });
    };

    const isPublic = variant === "public";
    const isActiveVisitPage = location.pathname.startsWith("/visit/active");
    const displayName = `${profile?.firstName?.trim() ?? ""} ${profile?.lastName?.trim() ?? ""}`.trim() || authEmail?.trim() || "Пользователь";
    return (
        <header className="w-full border-b border-slate-200 bg-white/90 backdrop-blur supports-[backdrop-filter]:bg-white/70">
          
            <div className="mx-auto flex max-w-6xl items-center justify-between gap-3 px-4 py-3 sm:px-6">
                <div className="flex items-center gap-3">
                    {!isPublic && !isSidebarOpen && onMenuToggle && (
                    <Hamburger
                        aria-label="Toggle sidebar"
                        onClick={onMenuToggle}
                    />
                )}
                    <h1 className="text-lg font-semibold tracking-tight text-slate-900 sm:text-xl">TimeCafe</h1>
                    {!isPublic && visitStatus === VisitUiStatus.Active && activeVisitDuration && (
                        <div className="hidden items-center gap-2 sm:flex">
                            <Tag appearance="brand" icon={<Clock20Regular />}>
                                {activeVisitDuration}
                            </Tag>
                            {!isActiveVisitPage && (
                                <Button appearance="secondary" onClick={() => navigate("/visit/active")}>
                                    К визиту
                                </Button>
                            )}
                        </div>
                    )}
                </div>

                {isPublic ? (
                    <div className="flex items-center gap-2">
                        <Button appearance="secondary" onClick={() => navigate("/login")}>Войти</Button>
                        <Button appearance="primary" onClick={() => navigate("/register")}>Регистрация</Button>
                    </div>
                ) : (
                    <div className="flex items-center gap-3">
                        <button
                            type="button"
                            className="rounded-full transition-transform duration-150 hover:scale-105 hover:brightness-95 active:scale-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400 focus-visible:ring-offset-2"
                            onClick={() => navigate("/personal-data")}
                            aria-label="Перейти в профиль"
                        >
                            <Avatar name={displayName} image={avatarPhotoUrl ? {src: avatarPhotoUrl} : undefined} color="colorful" />
                        </button>
                        <Button appearance="primary" onClick={handleServerLogout}>Выйти</Button>
                    </div>
                )}
            </div>
        </header>
    );
};
