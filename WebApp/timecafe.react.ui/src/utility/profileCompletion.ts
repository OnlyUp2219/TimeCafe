import type {Profile} from "../types/profile";

export const isNameCompleted = (profile: Profile | null): boolean => {
    if (!profile) return false;
    return Boolean(profile.firstName?.trim()) && Boolean(profile.lastName?.trim());
};

