import {httpClient} from "@api/httpClient";
import {normalizeUnknownError} from "@api/errors/normalize";
import type {Profile} from "@app-types/profile";

export interface UpdateProfileRequest {
    userId: string;
    firstName: string;
    lastName: string;
    middleName?: string | null;
    photoUrl?: string | null;
    birthDate?: string | null;
    gender: number;
}

export interface UpdateProfileResponse {
    message: string;
    profile: Profile;
}

export interface CreateEmptyProfileResponse {
    message: string;
}

export interface UploadProfilePhotoResponse {
    key: string;
    url: string;
    size: number;
    contentType: string;
}

export class ProfileApi {
    static async getProfileByUserId(userId: string): Promise<Profile> {
        try {
            const res = await httpClient.get<Profile>(`/userprofile/profiles/${userId}`);
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async createEmptyProfile(userId: string): Promise<CreateEmptyProfileResponse> {
        try {
            const res = await httpClient.post<CreateEmptyProfileResponse>(`/userprofile/profiles/empty/${userId}`);
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async updateProfile(profile: UpdateProfileRequest): Promise<UpdateProfileResponse> {
        try {
            const res = await httpClient.put<UpdateProfileResponse>("/userprofile/profiles", profile);
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }

    static async uploadProfilePhoto(userId: string, file: File): Promise<UploadProfilePhotoResponse> {
        try {
            if (!userId) throw new Error("Не найден userId пользователя");
            const formData = new FormData();
            formData.append("file", file, file.name);
            const res = await httpClient.post<UploadProfilePhotoResponse>(
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

    static async getProfilePhotoBlob(photoUrl: string): Promise<Blob> {
        try {
            if (!photoUrl) throw new Error("Не указан URL фото");
            const res = await httpClient.get<Blob>(photoUrl, { responseType: "blob" });
            return res.data;
        } catch (e) {
            throw normalizeUnknownError(e);
        }
    }
}
