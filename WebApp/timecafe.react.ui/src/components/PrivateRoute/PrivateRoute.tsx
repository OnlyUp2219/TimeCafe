import {Navigate} from "react-router-dom";
import * as React from "react";
import {refreshToken as refreshTokenApi} from "../../api/auth.ts";
import {type JSX, useState} from "react";
import {Spinner} from "@fluentui/react-components";
import {useDispatch, useSelector} from "react-redux";
import type {RootState} from "../../store";

interface PrivateRouteProps {
    children: JSX.Element;
}

export const PrivateRoute = ({children}: PrivateRouteProps) => {
    const [loading, setLoading] = useState(true);
    const [allowed, setAllowed] = useState(false);
    const dispatch = useDispatch();
    const accessToken = useSelector((state: RootState) => state.auth.accessToken);
    const refreshToken = useSelector((state: RootState) => state.auth.refreshToken);

    React.useEffect(() => {
        const checkAuth = async () => {
            await new Promise(resolve => setTimeout(resolve, 1000));


            if (accessToken) {
                setAllowed(true);
            } else if (refreshToken) {
                try {
                    await refreshTokenApi(refreshToken, dispatch);
                    setAllowed(true);
                } catch {
                    setAllowed(false);
                }
            } else {
                setAllowed(false);
            }
            setLoading(false);
        };

        checkAuth();
    }, []);

    if (loading) return <Spinner size={"huge"}/>;

    return allowed ? children : <Navigate to="/login" replace/>;
};
