import {Navigate} from "react-router-dom";
import {type JSX, useEffect, useState} from "react";
import {Spinner} from "@fluentui/react-components";
import {useDispatch, useSelector} from "react-redux";
import type {RootState} from "../../store";
import {authApi} from "../../shared/api/auth/authApi";
import {clearTokens, setAccessToken, setEmail, setRole, setUserId} from "../../store/authSlice";
import {getJwtInfo} from "../../shared/auth/jwt";

interface PrivateRouteProps {
    children: JSX.Element;
}

export const PrivateRoute = ({children}: PrivateRouteProps) => {
    const [loading, setLoading] = useState(true);
    const [allowed, setAllowed] = useState(false);
    const dispatch = useDispatch();
    const accessToken = useSelector((state: RootState) => state.auth.accessToken);

    useEffect(() => {
        const checkAuth = async () => {
            if (accessToken) {
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
                        setAllowed(true);
                    }
                } catch {
                    setAllowed(false);
                }
            }
            setLoading(false);
        };

        checkAuth();
    }, [dispatch, accessToken]);

    if (loading) return <Spinner size={"huge"}/>;

    return allowed ? children : <Navigate to="/login" replace/>;
};
