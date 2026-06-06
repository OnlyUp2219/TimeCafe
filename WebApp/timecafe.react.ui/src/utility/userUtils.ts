export const getUserFullName = (
    profile: { firstName?: string | null; lastName?: string | null; middleName?: string | null } | null | undefined,
    fallbackUserId?: string | null,
    isVisitWalkIn?: boolean
): string => {
    if (profile && (profile.firstName || profile.lastName)) {
        return [profile.lastName, profile.firstName, profile.middleName].filter(Boolean).join(" ");
    }
    
    if (fallbackUserId) {
        return "Зарегистрированный пользователь";
    }

    if (isVisitWalkIn !== undefined) {
        return isVisitWalkIn ? "Анонимный гость (Walk-in)" : "Зарегистрированный пользователь";
    }

    return "Анонимный гость (Walk-in)";
};

import { Gender, ProfileStatus } from "@app-types/profile";

export const genderLabel = (gender?: number) => {
    switch (gender) {
        case Gender.Male:
            return "Мужской";
        case Gender.Female:
            return "Женский";
        default:
            return "Не указан";
    }
};

export const profileStatusLabel = (status?: number) => {
    switch (status) {
        case ProfileStatus.Pending:
            return "Ожидает заполнения";
        case ProfileStatus.Completed:
            return "Заполнен";
        case ProfileStatus.Banned:
            return "Заблокирован";
        default:
            return "Неизвестно";
    }
};

export const profileStatusColor = (status?: number): "warning" | "success" | "danger" => {
    switch (status) {
        case ProfileStatus.Pending:
            return "warning";
        case ProfileStatus.Completed:
            return "success";
        case ProfileStatus.Banned:
            return "danger";
        default:
            return "warning";
    }
};
