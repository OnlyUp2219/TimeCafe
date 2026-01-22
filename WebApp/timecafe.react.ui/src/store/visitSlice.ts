import {createSlice, type PayloadAction} from "@reduxjs/toolkit";

type VisitState = {
    selectedTariffId: string | null;
};

const initialState: VisitState = {
    selectedTariffId: "standard",
};

const visitSlice = createSlice({
    name: "visit",
    initialState,
    reducers: {
        setSelectedTariffId: (state, action: PayloadAction<string | null>) => {
            state.selectedTariffId = action.payload;
        },
        clearSelectedTariffId: (state) => {
            state.selectedTariffId = null;
        },
    },
});

export const {setSelectedTariffId, clearSelectedTariffId} = visitSlice.actions;
export default visitSlice.reducer;
