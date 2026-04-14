import {createSlice, type PayloadAction} from "@reduxjs/toolkit";
import type {Permission} from "@shared/auth/permissions";

interface PermissionsState {
    items: string[];
    loaded: boolean;
}

const initialState: PermissionsState = {
    items: [],
    loaded: false,
};

const permissionsSlice = createSlice({
    name: "permissions",
    initialState,
    reducers: {
        setPermissions: (state, action: PayloadAction<string[]>) => {
            state.items = action.payload;
            state.loaded = true;
        },
        clearPermissions: (state) => {
            state.items = [];
            state.loaded = false;
        },
    },
});

export const {setPermissions, clearPermissions} = permissionsSlice.actions;

export const selectPermissions = (state: {permissions: PermissionsState}) => state.permissions.items;
export const selectPermissionsLoaded = (state: {permissions: PermissionsState}) => state.permissions.loaded;
export const selectHasPermission = (permission: Permission) =>
    (state: {permissions: PermissionsState}) => state.permissions.items.includes(permission);
export const selectHasAnyPermission = (permissions: Permission[]) =>
    (state: {permissions: PermissionsState}) => permissions.some(p => state.permissions.items.includes(p));

export default permissionsSlice.reducer;
