import {createApi} from "@reduxjs/toolkit/query/react";
import {baseQueryWithReauth} from "@store/api/baseQuery";
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

export const profileApi = createApi({
    reducerPath: "profileApi",
    baseQuery: baseQueryWithReauth,
    tagTypes: ["Profile", "ProfilePhoto"],
    endpoints: (builder) => ({
        getProfileByUserId: builder.query<Profile, string>({
            async queryFn(userId, _queryApi, _extraOptions, fetchWithBQ) {
                const result = await fetchWithBQ(`/userprofile/profiles/${userId}`);
                if (result.error) {
                    const status = (result.error as { status?: number }).status;
                    if (status === 404) {
                        const createResult = await fetchWithBQ({
                            url: `/userprofile/profiles/empty/${userId}`,
                            method: "POST",
                        });
                        if (createResult.error) return {error: createResult.error};
                        const retryResult = await fetchWithBQ(`/userprofile/profiles/${userId}`);
                        if (retryResult.error) return {error: retryResult.error};
                        return {data: retryResult.data as Profile};
                    }
                    return {error: result.error};
                }
                return {data: result.data as Profile};
            },
            providesTags: (_result, _error, userId) => [{type: "Profile", id: userId}],
        }),

        createEmptyProfile: builder.mutation<CreateEmptyProfileResponse, string>({
            query: (userId) => ({
                url: `/userprofile/profiles/empty/${userId}`,
                method: "POST",
            }),
            invalidatesTags: (_result, _error, userId) => [{type: "Profile", id: userId}],
        }),

        updateProfile: builder.mutation<UpdateProfileResponse, UpdateProfileRequest>({
            query: (body) => ({
                url: `/userprofile/profiles/${body.userId}`,
                method: "PUT",
                body,
            }),
            invalidatesTags: (_result, _error, arg) => [{type: "Profile", id: arg.userId}],
        }),

        uploadProfilePhoto: builder.mutation<UploadProfilePhotoResponse, { userId: string; file: File }>({
            query: ({userId, file}) => {
                const formData = new FormData();
                formData.append("file", file, file.name);
                return {
                    url: `/userprofile/S3/image/${userId}`,
                    method: "POST",
                    body: formData,
                };
            },
            invalidatesTags: (_result, _error, arg) => [
                {type: "Profile", id: arg.userId},
                {type: "ProfilePhoto", id: arg.userId},
            ],
        }),

        deleteProfilePhoto: builder.mutation<void, string>({
            query: (userId) => ({
                url: `/userprofile/S3/image/${userId}`,
                method: "DELETE",
            }),
            invalidatesTags: (_result, _error, userId) => [
                {type: "Profile", id: userId},
                {type: "ProfilePhoto", id: userId},
            ],
        }),
    }),
});

export const {
    useGetProfileByUserIdQuery,
    useLazyGetProfileByUserIdQuery,
    useCreateEmptyProfileMutation,
    useUpdateProfileMutation,
    useUploadProfilePhotoMutation,
    useDeleteProfilePhotoMutation,
} = profileApi;
