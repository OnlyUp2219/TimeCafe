import type {RootState} from "@store";

export const selectAccessToken = (state: RootState) => state.auth.accessToken;
export const selectUserId = (state: RootState) => state.auth.userId;
export const selectEmail = (state: RootState) => state.auth.email;
export const selectEmailConfirmed = (state: RootState) => state.auth.emailConfirmed;
export const selectPhoneNumber = (state: RootState) => state.auth.phoneNumber;
export const selectPhoneNumberConfirmed = (state: RootState) => state.auth.phoneNumberConfirmed;
export const selectRole = (state: RootState) => state.auth.role;
