import {createSlice} from "@reduxjs/toolkit";

interface AuthState {
    accessToken: string;
    refreshToken: string;
}

const initialState: AuthState = {
    accessToken: "",
    refreshToken: "",
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
        clearAccessToken: (state) => {
            state.accessToken = "";
        },
        clearRefreshToken: (state) => {
            state.refreshToken = "";
        },
        clearTokens: (state) => {
            state.accessToken = "";
            state.refreshToken = "";
        }
    }
})


export const {setAccessToken, setRefreshToken, clearAccessToken, clearRefreshToken, clearTokens} = authSlice.actions;
export default authSlice.reducer;
