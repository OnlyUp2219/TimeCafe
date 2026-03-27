import {httpClient} from "@api/httpClient";
import {normalizeUnknownError} from "@api/errors/normalize";

export class ProfileApi {
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
