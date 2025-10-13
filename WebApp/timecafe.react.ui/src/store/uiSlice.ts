import {createSlice} from "@reduxjs/toolkit";


interface uiState {
    isSideBarOpen: boolean;
    selectedNav: string;
}

const initialState: uiState = {
    isSideBarOpen: false,
    selectedNav: "1",
}

const uiSlice = createSlice({
    name: 'ui',
    initialState,
    reducers: {
        toggleSidebar: (state) => {
            state.isSideBarOpen = !state.isSideBarOpen;
        },
        setSidebarOpen: (state, action) => {
            state.isSideBarOpen = action.payload;
        },
        setSelectedNav: (state, action) => {
            state.selectedNav = action.payload;
        }
    },
});

export const {toggleSidebar, setSidebarOpen, setSelectedNav} = uiSlice.actions;
export default uiSlice.reducer;