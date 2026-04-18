// TODO: рассмотреть возможность перехода на запрос есть ли у роли возможность перейти в админ панель!

import {Navigate} from "react-router-dom";
import {type JSX, useEffect, useState} from "react";
import {Spinner} from "@fluentui/react-components";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {tryRefreshAccessToken} from "@shared/auth/refreshToken";
import {
    clearTokens,
    setAccessToken,
    setEmail,
    setRole,
    setUserId
} from "@store/authSlice";
import {getJwtInfo} from "@shared/auth/jwt";
import {hydrateAuthFromCurrentUser} from "@shared/auth/hydrateAuthFromCurrentUser";
import {setPermissions, clearPermissions} from "@store/permissionsSlice";
import {AdminPanelPermission} from "@shared/auth/permissions";
import {authApi} from "@store/api/authApi";

interface AdminRouteProps {
    children: JSX.Element;
}

export const AdminRoute = ({children}: AdminRouteProps) => {
    const [loading, setLoading] = useState(true);
    const [allowed, setAllowed] = useState(false);
    const dispatch = useAppDispatch();
    const accessToken = useAppSelector((state) => state.auth.accessToken);

    useEffect(() => {
        const hydrateQuietly = async () => {
            try {
                await hydrateAuthFromCurrentUser(dispatch);
            } catch {  }
        };

        const loadPermissions = async (): Promise<string[]> => {
            try {
                const result = await dispatch(authApi.endpoints.getMyPermissions.initiate()).unwrap();
                dispatch(setPermissions(result.permissions));
                return result.permissions;
            } catch {
                dispatch(clearPermissions());
                return [];
            }
        };

        const checkAuth = async () => {
            let token: string | null = accessToken;

            if (!token) {
                try {
                    token = await tryRefreshAccessToken();
                    if (!token) {
                        dispatch(clearTokens());
                        setAllowed(false);
                        setLoading(false);
                        return;
                    }
                    dispatch(setAccessToken(token));
                    const info = getJwtInfo(token);
                    if (info.userId) dispatch(setUserId(info.userId));
                    if (info.role) dispatch(setRole(info.role));
                    if (info.email) dispatch(setEmail(info.email));
                } catch {
                    setAllowed(false);
                    setLoading(false);
                    return;
                }
            }

            await hydrateQuietly();
            const perms = await loadPermissions();
            setAllowed(perms.includes(AdminPanelPermission));
            setLoading(false);
        };

        void checkAuth();
    }, [dispatch, accessToken]);

    if (loading) return <Spinner size={"huge"}/>;

    return allowed ? children : <Navigate to="/login" replace/>;
};