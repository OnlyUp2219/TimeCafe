import {httpClient} from "../httpClient";
import type {Profile} from "../../../types/profile";

export class ProfileApi {
    static async getProfileByUserId(userId: string): Promise<Profile> {
        const res = await httpClient.get<Profile>(`/userprofile/profiles/${userId}`);
        return res.data;
    }

    static async createEmptyProfile(userId: string): Promise<void> {
        await httpClient.post(`/userprofile/profiles/empty/${userId}`);
    }

    static async updateProfile(profile: {
        userId: string;
        firstName: string;
        lastName: string;
        middleName?: string | null;
        accessCardNumber?: string | null;
        photoUrl?: string | null;
        birthDate?: string | null;
        gender: number;
    }): Promise<Profile> {
        const res = await httpClient.put<{ message: string; profile: Profile }>("/userprofile/profiles", profile);
        return res.data.profile;
    }
}
