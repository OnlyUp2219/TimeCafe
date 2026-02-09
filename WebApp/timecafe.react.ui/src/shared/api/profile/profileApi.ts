import {httpClient} from "../httpClient";
import {normalizeUnknownError} from "../errors/normalize";
import type {Profile} from "@app-types/profile";

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
        photoUrl?: string | null;
        birthDate?: string | null;
        gender: number;
    }): Promise<Profile> {
        const res = await httpClient.put<{ message: string; profile: Profile }>("/userprofile/profiles", profile);
        return res.data.profile;
    }

    static async uploadProfilePhoto(userId: string, file: File): Promise<{
        key: string;
        url: string;
        size: number;
        contentType: string;
    }> {
        try {
            if (!userId) throw new Error("Не найден userId пользователя");
            const formData = new FormData();
            formData.append("file", file, file.name);
            const res = await httpClient.post<{ key: string; url: string; size: number; contentType: string }>(
                `/userprofile/S3/image/${userId}`,
                formData,
                {
                    headers: {
                        "Content-Type": "multipart/form-data",
                    },
                }
            );
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async deleteProfilePhoto(userId: string): Promise<void> {
        try {
            if (!userId) throw new Error("Не найден userId пользователя");
            await httpClient.delete(`/userprofile/S3/image/${userId}`);
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }
}
