import {combineReducers, configureStore} from "@reduxjs/toolkit";
import {persistReducer, persistStore} from "redux-persist";
import uiSlice from "./uiSlice.ts";
import storage from "redux-persist/lib/storage";
import authSlice from "./authSlice.ts";

const rootReducer = combineReducers({
    ui: uiSlice,
    auth: authSlice,
});

const persistConfigure = {key: 'root', storage};

const persistedReducer = persistReducer(persistConfigure, rootReducer);

export const store = configureStore({
    reducer: persistedReducer,
    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware({
            serializableCheck: {
                ignoredActions: [
                    "persist/PERSIST",
                    "persist/REHYDRATE",
                    "persist/REGISTER",
                    "persist/FLUSH",
                    "persist/PAUSE",
                    "persist/PURGE",
                ],
            },
        }),
});


export const persistor = persistStore(store);
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
