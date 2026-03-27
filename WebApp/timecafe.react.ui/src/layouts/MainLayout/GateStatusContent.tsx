import type {FC} from "react";
import {Body1, Button, Caption1, Spinner} from "@fluentui/react-components";

interface GateStatusContentProps {
    effectiveUserId: string;
    profileLoading: boolean;
    profileError: string | null;
    loadingTimedOut: boolean;
    hasProfile: boolean;
    onLogout: () => void;
    onRetry: () => void;
}

export const GateStatusContent: FC<GateStatusContentProps> = ({
    effectiveUserId,
    profileLoading,
    profileError,
    loadingTimedOut,
    hasProfile,
    onLogout,
    onRetry,
}) => {
    if (!effectiveUserId) {
        return (
            <div className="flex flex-col gap-3">
                <Body1>Не удалось определить пользователя из сессии.</Body1>
                <Caption1>Попробуйте перезайти в систему.</Caption1>
                <Button appearance="primary" onClick={onLogout}>
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
                <Button appearance="primary" onClick={onRetry}>
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

    if (!hasProfile && profileError) {
        return (
            <div className="flex flex-col gap-3">
                <Body1>Не удалось загрузить профиль. Повторите попытку позже.</Body1>
                <Caption1>{profileError}</Caption1>
                <Button appearance="primary" onClick={onRetry}>
                    Повторить
                </Button>
            </div>
        );
    }

    if (!hasProfile) {
        return (
            <div className="flex items-center gap-3">
                <Spinner size="small"/>
                <Body1>Подготавливаем профиль…</Body1>
            </div>
        );
    }

    return null;
};
