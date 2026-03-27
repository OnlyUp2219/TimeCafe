import {Navigate} from "react-router-dom";
import {type JSX, useEffect, useState} from "react";
import {Spinner} from "@fluentui/react-components";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {authApi} from "@api/auth/authApi";
import {
    clearTokens,
    setAccessToken,
    setEmail,
    setRole,
    setUserId
} from "@store/authSlice";
import {getJwtInfo} from "@shared/auth/jwt";
import {hydrateAuthFromCurrentUser} from "@shared/auth/hydrateAuthFromCurrentUser";

interface PrivateRouteProps {
    children: JSX.Element;
}

export const PrivateRoute = ({children}: PrivateRouteProps) => {
    const [loading, setLoading] = useState(true);
    const [allowed, setAllowed] = useState(false);
    const dispatch = useAppDispatch();
    const accessToken = useAppSelector((state) => state.auth.accessToken);

    useEffect(() => {
        const hydrateQuietly = async () => {
            try {
                await hydrateAuthFromCurrentUser(dispatch);
            } catch { /* гидратация необязательна — профиль загрузится позже */ }
        };

        const checkAuth = async () => {
            if (accessToken) {
                await hydrateQuietly();
                setAllowed(true);
            } else {
                try {
                    const token = await authApi.tryRefreshAccessToken();
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
                        setAllowed(true);
                    }
                } catch {
                    setAllowed(false);
                }
            }
            setLoading(false);
        };

        void checkAuth();
    }, [dispatch, accessToken]);

    if (loading) return <Spinner size={"huge"}/>;

    return allowed ? children : <Navigate to="/login" replace/>;
};
