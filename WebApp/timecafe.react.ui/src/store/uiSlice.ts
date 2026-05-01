import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

export interface PaginationState {
    page: number;
    size: number;
    filters?: Record<string, any>;
}

interface uiState {
    isSideBarOpen: boolean;
    isSidebarCollapsed: boolean;
    selectedNav: string;
    pagination: Record<string, PaginationState>;
}

const initialState: uiState = {
    isSideBarOpen: false,
    isSidebarCollapsed: false,
    selectedNav: "1",
    pagination: {},
}

const uiSlice = createSlice({
    name: 'ui',
    initialState,
    reducers: {
        toggleSidebar: (state) => {
            state.isSideBarOpen = !state.isSideBarOpen;
        },
        setSidebarOpen: (state, action: PayloadAction<boolean>) => {
            state.isSideBarOpen = action.payload;
        },
        setSidebarCollapsed: (state, action: PayloadAction<boolean>) => {
            state.isSidebarCollapsed = action.payload;
        },
        setSelectedNav: (state, action: PayloadAction<string>) => {
            state.selectedNav = action.payload;
        },
        setPagination: (state, action: PayloadAction<{ key: string; page: number; size: number }>) => {
            const { key, page, size } = action.payload;
            if (!state.pagination[key]) {
                state.pagination[key] = { page, size };
            } else {
                state.pagination[key].page = page;
                state.pagination[key].size = size;
            }
        },
        setFilters: (state, action: PayloadAction<{ key: string; filters: Record<string, any> }>) => {
            const { key, filters } = action.payload;
            if (state.pagination[key]) {
                state.pagination[key].filters = { ...state.pagination[key].filters, ...filters };
                state.pagination[key].page = 1;
            } else {
                state.pagination[key] = { page: 1, size: 20, filters };
            }
        }
    },
});

export const { toggleSidebar, setSidebarOpen, setSidebarCollapsed, setSelectedNav, setPagination, setFilters } = uiSlice.actions;
export default uiSlice.reducer;