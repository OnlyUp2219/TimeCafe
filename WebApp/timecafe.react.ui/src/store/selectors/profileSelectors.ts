import type {RootState} from "@store";

export const selectProfile = (state: RootState) => state.profile.data;
export const selectProfileLoading = (state: RootState) => state.profile.loading;
export const selectProfileSaving = (state: RootState) => state.profile.saving;
export const selectProfileError = (state: RootState) => state.profile.error;
export const selectProfileLoadedUserId = (state: RootState) => state.profile.loadedUserId;
