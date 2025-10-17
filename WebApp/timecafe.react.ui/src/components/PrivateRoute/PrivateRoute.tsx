import {Navigate} from "react-router-dom";
import {refreshToken} from "../api/auth"; // твоя функция обновления
import * as React from "react";

interface PrivateRouteProps {
    children: JSX.Element;
}

export const PrivateRoute = ({children}: PrivateRouteProps) => {
    const [loading, setLoading] = React.useState(true);
    const [allowed, setAllowed] = React.useState(false);

    React.useEffect(() => {
        const checkAuth = async () => {
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

    if (loading) return <div>Проверка авторизации...</div>;

    return allowed ? children : <Navigate to="/login" replace/>;
};
