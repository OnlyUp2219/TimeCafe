import type {RootState} from "@store";

export const selectIsSidebarOpen = (state: RootState) => state.ui.isSideBarOpen;
export const selectSelectedNav = (state: RootState) => state.ui.selectedNav;
