import {combineReducers, configureStore} from "@reduxjs/toolkit";
import {persistReducer, persistStore} from "redux-persist";
import uiSlice from "@store/uiSlice";
import storage from "redux-persist/lib/storage";
import authSlice from "@store/authSlice";
import {clearTokens} from "@store/authSlice";
import profileSlice from "@store/profileSlice";
import visitSlice from "@store/visitSlice";
import billingSlice from "@store/billingSlice";
import {authApi} from "@store/api/authApi";
import {profileApi} from "@store/api/profileApi";
import {billingApi} from "@store/api/billingApi";
import {venueApi} from "@store/api/venueApi";

const appReducer = combineReducers({
    ui: uiSlice,
    auth: authSlice,
    profile: profileSlice,
    visit: visitSlice,
    billing: billingSlice,
    [authApi.reducerPath]: authApi.reducer,
    [profileApi.reducerPath]: profileApi.reducer,
    [billingApi.reducerPath]: billingApi.reducer,
    [venueApi.reducerPath]: venueApi.reducer,
});

const rootReducer = (state: ReturnType<typeof appReducer> | undefined, action: { type: string }) => {
    if (action.type === clearTokens.type) {
        return appReducer(undefined, action);
    }

    return appReducer(state, action);
};

const persistConfigure = {
    key: "root-v4",
    storage,
    blacklist: [
        "auth",
        authApi.reducerPath,
        profileApi.reducerPath,
        billingApi.reducerPath,
        venueApi.reducerPath,
    ],
};

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
        }).concat(
            authApi.middleware,
            profileApi.middleware,
            billingApi.middleware,
            venueApi.middleware,
        ),
});


export const persistor = persistStore(store);
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
