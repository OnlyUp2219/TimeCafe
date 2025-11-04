import {createSlice} from "@reduxjs/toolkit";

interface AuthState {
    accessToken: string;
    refreshToken: string;
    email: string;
    emailConfirmed: boolean;
}

const initialState: AuthState = {
    accessToken: "",
    refreshToken: "",
    email: "",
    emailConfirmed: false
}

const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        setAccessToken: (state, action) => {
            state.accessToken = action.payload;
        },
        setRefreshToken: (state, action) => {
            state.refreshToken = action.payload;
        },
        setEmail: (state, action) => {
            state.email = action.payload;
        },
        setEmailConfirmed: (state, action) => {
            state.emailConfirmed = action.payload;
        },
        clearAccessToken: (state) => {
            state.accessToken = "";
        },
        clearRefreshToken: (state) => {
            state.refreshToken = "";
        },
        clearTokens: (state) => {
            state.accessToken = "";
            state.refreshToken = "";
            state.emailConfirmed = false;
        }
    }
})

export const {setAccessToken, setRefreshToken, setEmail, setEmailConfirmed, clearAccessToken, clearRefreshToken, clearTokens} = authSlice.actions;
export default authSlice.reducer;
