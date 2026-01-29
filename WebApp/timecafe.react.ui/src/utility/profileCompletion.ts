import type {Profile} from "../types/profile";

export type ProfileUiStatus = "Pending" | "Completed";

export const isNameCompleted = (profile: Profile | null): boolean => {
    if (!profile) return false;
    return Boolean(profile.firstName?.trim()) && Boolean(profile.lastName?.trim());
};

export const isPhoneCompleted = (profile: Profile | null): boolean => {
    if (!profile) return false;
    return Boolean(profile.phoneNumber?.trim());
};

export const getProfileUiStatus = (profile: Profile | null): ProfileUiStatus => {
    return isNameCompleted(profile) ? "Completed" : "Pending";
};
