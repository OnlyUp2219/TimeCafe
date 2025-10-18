import {Navigate} from "react-router-dom";
import * as React from "react";
import {refreshToken} from "../../api/auth.ts";
import type {JSX} from "react";
import {Spinner} from "@fluentui/react-components";

interface PrivateRouteProps {
    children: JSX.Element;
}

export const PrivateRoute = ({children}: PrivateRouteProps) => {
    const [loading, setLoading] = React.useState(true);
    const [allowed, setAllowed] = React.useState(false);

    React.useEffect(() => {
        const checkAuth = async () => {
            await new Promise(resolve => setTimeout(resolve, 1000));

            const access = localStorage.getItem("accessToken");
            const refresh = localStorage.getItem("refreshToken");

            if (access) {
                setAllowed(true);
            } else if (refresh) {
                try {
                    await refreshToken();
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
