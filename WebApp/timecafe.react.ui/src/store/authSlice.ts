import {createSlice} from "@reduxjs/toolkit";

interface AuthState {
    accessToken: string;
    userId: string;
    role: string;
    email: string;
    emailConfirmed: boolean;
    phoneNumber: string;
    phoneNumberConfirmed: boolean;
}

const initialState: AuthState = {
    accessToken: "",
    userId: "",
    role: "",
    email: "",
    emailConfirmed: false,
    phoneNumber: "",
    phoneNumberConfirmed: false
}

const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        setAccessToken: (state, action) => {
            state.accessToken = action.payload;
        },
        setUserId: (state, action) => {
            state.userId = action.payload;
        },
        setRole: (state, action) => {
            state.role = action.payload;
        },
        setEmail: (state, action) => {
            state.email = action.payload;
        },
        setEmailConfirmed: (state, action) => {
            state.emailConfirmed = action.payload;
        },
        setPhoneNumber: (state, action) => {
            state.phoneNumber = action.payload;
        },
        setPhoneNumberConfirmed: (state, action) => {
            state.phoneNumberConfirmed = action.payload;
        },
        clearAccessToken: (state) => {
            state.accessToken = "";
        },
        clearTokens: (state) => {
            state.accessToken = "";
            state.userId = "";
            state.role = "";
            state.email = "";
            state.emailConfirmed = false;
            state.phoneNumber = "";
            state.phoneNumberConfirmed = false;
        }
    }
})

export const {
    setAccessToken,
    setUserId,
    setRole,
    setEmail,
    setEmailConfirmed,
    setPhoneNumber,
    setPhoneNumberConfirmed,
    clearAccessToken,
    clearTokens
} = authSlice.actions;
export default authSlice.reducer;
