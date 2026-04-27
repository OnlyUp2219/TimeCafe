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

export interface GetProfilesPageResponse {
    profiles: Profile[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
}

export const profileApi = createApi({
    reducerPath: "profileApi",
    baseQuery: baseQueryWithReauth,
    tagTypes: ["Profile", "ProfilePhoto", "ProfilesPage"],
    endpoints: (builder) => ({
        getProfileByUserId: builder.query<Profile, string>({
            async queryFn(userId, _queryApi, _extraOptions, fetchWithBQ) {
                const result = await fetchWithBQ(`/userprofile/profiles/${userId}`);
                if (result.error) {
                    const status = (result.error as {status?: number}).status;
                    if (status === 404) {
                        const createResult = await fetchWithBQ({
                            url: `/userprofile/profiles/empty/${userId}`,
                            method: "POST",
                        });
                        if (createResult.error) return {error: createResult.error};
                        const retryResult = await fetchWithBQ(`/userprofile/profiles/${userId}`);
                        if (retryResult.error) return {error: retryResult.error};
                        
                        const retryData = retryResult.data as {profile: Profile} | Profile;
                        const retryProfile = "profile" in retryData ? retryData.profile : (retryData as Profile);
                        return {data: retryProfile};
                    }
                    return {error: result.error};
                }
                
                const data = result.data as {profile: Profile} | Profile;
                const profile = "profile" in data ? data.profile : (data as Profile);
                return {data: profile};
            },
            providesTags: (_result, _error, userId) => [{type: "Profile", id: userId}],
        }),

        getProfileByUserIdReadOnly: builder.query<Profile | null, string>({
            query: (userId) => `/userprofile/profiles/${userId}`,
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
        getAdditionalInfosByUserId: builder.query<{ items: { infoId: string; userId: string; infoText: string; createdBy: string; createdAt: string }[]; totalCount: number }, { userId: string; pageNumber: number; pageSize: number }>({
            query: ({ userId, pageNumber, pageSize }) => ({
                url: `/userprofile/profiles/${userId}/infos`,
                params: { pageNumber, pageSize },
            }),
            providesTags: (_result, _error, arg) => [{ type: "Profile", id: `infos-${arg.userId}` }],
        }),

        createAdditionalInfo: builder.mutation<
            {message: string; info: {infoId: string; userId: string; infoText: string; createdBy: string; createdAt: string}},
            {userId: string; infoText: string; createdBy?: string}
        >({
            query: (body) => ({
                url: "/userprofile/infos",
                method: "POST",
                body,
            }),
            invalidatesTags: (_result, _error, arg) => [{type: "Profile", id: `infos-${arg.userId}`}],
        }),

        deleteAdditionalInfo: builder.mutation<{message: string}, {infoId: string; userId: string}>({
            query: ({infoId}) => ({
                url: `/userprofile/infos/${infoId}`,
                method: "DELETE",
            }),
            invalidatesTags: (_result, _error, arg) => [{type: "Profile", id: `infos-${arg.userId}`}],
        }),

        getProfilesPage: builder.query<GetProfilesPageResponse, {pageNumber: number; pageSize: number}>({
            query: ({pageNumber, pageSize}) => ({
                url: "/userprofile/profiles/page",
                params: {pageNumber, pageSize},
            }),
            providesTags: ["ProfilesPage"],
        }),

        deleteProfile: builder.mutation<{message: string}, string>({
            query: (userId) => ({
                url: `/userprofile/profiles/${userId}`,
                method: "DELETE",
            }),
            invalidatesTags: (_result, _error, userId) => [
                {type: "Profile", id: userId},
                "ProfilesPage",
            ],
        }),
    }),
});

export const {
    useGetProfileByUserIdQuery,
    useLazyGetProfileByUserIdQuery,
    useGetProfileByUserIdReadOnlyQuery,
    useCreateEmptyProfileMutation,
    useUpdateProfileMutation,
    useUploadProfilePhotoMutation,
    useDeleteProfilePhotoMutation,
    useGetAdditionalInfosByUserIdQuery,
    useCreateAdditionalInfoMutation,
    useDeleteAdditionalInfoMutation,
    useGetProfilesPageQuery,
    useDeleteProfileMutation,
} = profileApi;
