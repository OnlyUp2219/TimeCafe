import {authApi} from "../api/auth/authApi";
import {setEmail, setEmailConfirmed, setPhoneNumber, setPhoneNumberConfirmed, setUserId} from "../../store/authSlice";
import type {Dispatch} from "redux";

export const hydrateAuthFromCurrentUser = async (dispatch: Dispatch) => {
    const currentUser = await authApi.getCurrentUser();
    if (currentUser.userId) dispatch(setUserId(currentUser.userId));
    dispatch(setEmail(currentUser.email));
    dispatch(setEmailConfirmed(currentUser.emailConfirmed));
    dispatch(setPhoneNumber(currentUser.phoneNumber ?? ""));
    dispatch(setPhoneNumberConfirmed(currentUser.phoneNumberConfirmed));
};
