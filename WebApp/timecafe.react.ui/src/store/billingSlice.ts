import {createSlice} from "@reduxjs/toolkit";
import {clearTokens} from "@store/authSlice";

type BillingState = {
    error: string | null;
};

const initialState: BillingState = {
    error: null,
};

const billingSlice = createSlice({
    name: "billing",
    initialState,
    reducers: {
        clearBillingError: (state) => {
            state.error = null;
        },
    },
    extraReducers: (builder) => {
        builder.addCase(clearTokens, () => initialState);
    },
});

export const {clearBillingError} = billingSlice.actions;

export default billingSlice.reducer;
