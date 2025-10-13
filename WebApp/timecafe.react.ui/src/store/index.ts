import {configureStore} from "@reduxjs/toolkit";
import {persistReducer, persistStore} from "redux-persist";
import uiSlice from "./uiSlice.ts";
import storage from "redux-persist/lib/storage";

const persistConfigure = {key: 'root', storage};

const persistedReducer = persistReducer(persistConfigure, uiSlice);

export const store = configureStore({
    reducer: {
        ui: persistedReducer,
    },
});

export const persistor = persistStore(store);
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
