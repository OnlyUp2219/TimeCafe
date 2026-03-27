import {createSlice, type PayloadAction} from "@reduxjs/toolkit";
import {clearTokens} from "@store/authSlice";

type VisitState = {
    selectedTariffId: string | null;
};

const initialState: VisitState = {
    selectedTariffId: null,
};

const visitSlice = createSlice({
    name: "visit",
    initialState,
    reducers: {
        setSelectedTariffId: (state, action: PayloadAction<string | null>) => {
            state.selectedTariffId = action.payload;
        },
    },
    extraReducers: (builder) => {
        builder.addCase(clearTokens, () => initialState);
    },
});

export const {setSelectedTariffId} = visitSlice.actions;

export default visitSlice.reducer;
