import {authApi as authApiSlice} from "@store/api/authApi";
import {setEmail, setEmailConfirmed, setPhoneNumber, setPhoneNumberConfirmed, setUserId} from "@store/authSlice";
import type {AppDispatch} from "@store";

export const hydrateAuthFromCurrentUser = async (dispatch: AppDispatch) => {
    const result = await dispatch(authApiSlice.endpoints.getCurrentUser.initiate());
    const currentUser = result.data;
    if (!currentUser) throw new Error("Failed to fetch current user");
    if (currentUser.userId) dispatch(setUserId(currentUser.userId));
    dispatch(setEmail(currentUser.email));
    dispatch(setEmailConfirmed(currentUser.emailConfirmed));
    dispatch(setPhoneNumber(currentUser.phoneNumber ?? ""));
    dispatch(setPhoneNumberConfirmed(currentUser.phoneNumberConfirmed));
};
