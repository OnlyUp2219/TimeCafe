// TODO: рассмотреть возможность перехода на запрос есть ли у роли возможность перейти в админ панель!

import {Navigate} from "react-router-dom";
import {type JSX, useEffect, useState} from "react";
import {Spinner} from "@fluentui/react-components";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {tryRefreshAccessToken} from "@shared/auth/refreshToken";
import {Roles} from "@shared/auth/roles";
import {
    clearTokens,
    setAccessToken,
    setEmail,
    setRole,
    setUserId
} from "@store/authSlice";
import {getJwtInfo} from "@shared/auth/jwt";
import {hydrateAuthFromCurrentUser} from "@shared/auth/hydrateAuthFromCurrentUser";

interface AdminRouteProps {
    children: JSX.Element;
}

export const AdminRoute = ({children}: AdminRouteProps) => {
    const [loading, setLoading] = useState(true);
    const [allowed, setAllowed] = useState(false);
    const dispatch = useAppDispatch();
    const accessToken = useAppSelector((state) => state.auth.accessToken);
    const role = useAppSelector((state) => state.auth.role);

    useEffect(() => {
        const hydrateQuietly = async () => {
            try {
                await hydrateAuthFromCurrentUser(dispatch);
            } catch {  }
        };

        const checkAuth = async () => {
            if (accessToken) {
                await hydrateQuietly();
                const isAdmin = role === Roles.Admin;
                setAllowed(isAdmin);
            } else {
                try {
                    const token = await tryRefreshAccessToken();
                    if (!token) {
                        dispatch(clearTokens());
                        setAllowed(false);
                    } else {
                        dispatch(setAccessToken(token));
                        const info = getJwtInfo(token);
                        if (info.userId) dispatch(setUserId(info.userId));
                        if (info.role) dispatch(setRole(info.role));
                        if (info.email) dispatch(setEmail(info.email));
                        await hydrateQuietly();
                        const isAdmin = info.role === Roles.Admin;
                        setAllowed(isAdmin);
                    }
                } catch {
                    setAllowed(false);
                }
            }
            setLoading(false);
        };

        void checkAuth();
    }, [dispatch, accessToken, role]);

    if (loading) return <Spinner size={"huge"}/>;

    return allowed ? children : <Navigate to="/login" replace/>;
};