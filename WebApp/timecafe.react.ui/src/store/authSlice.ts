import {createSlice} from "@reduxjs/toolkit";

interface AuthState {
    accessToken: string;
    email: string;
    emailConfirmed: boolean;
}

const initialState: AuthState = {
    accessToken: "",
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
        setEmail: (state, action) => {
            state.email = action.payload;
        },
        setEmailConfirmed: (state, action) => {
            state.emailConfirmed = action.payload;
        },
        clearAccessToken: (state) => {
            state.accessToken = "";
        },
        clearTokens: (state) => {
            state.accessToken = "";
            state.emailConfirmed = false;
        }
    }
})

export const {setAccessToken, setEmail, setEmailConfirmed, clearAccessToken, clearTokens} = authSlice.actions;
export default authSlice.reducer;
