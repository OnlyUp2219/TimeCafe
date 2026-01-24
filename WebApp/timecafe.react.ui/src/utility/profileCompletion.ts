import type {ClientInfo} from "../types/client";

export type ProfileUiStatus = "Pending" | "Completed";

export const isNameCompleted = (client: ClientInfo | null): boolean => {
    if (!client) return false;
    return Boolean(client.firstName?.trim()) && Boolean(client.lastName?.trim());
};

export const isPhoneCompleted = (client: ClientInfo | null): boolean => {
    if (!client) return false;
    return Boolean(client.phoneNumber?.trim()) && client.phoneNumberConfirmed === true;
};

export const getProfileUiStatus = (client: ClientInfo | null): ProfileUiStatus => {
    return isNameCompleted(client) && isPhoneCompleted(client) ? "Completed" : "Pending";
};
