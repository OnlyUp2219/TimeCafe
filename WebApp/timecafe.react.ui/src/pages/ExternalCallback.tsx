import * as React from "react";
import {useDispatch} from "react-redux";
import {useNavigate} from "react-router-dom";
import {setAccessToken, setRefreshToken} from "../store/authSlice.ts";
import {Spinner} from "@fluentui/react-components";

export const ExternalCallback = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();

    React.useEffect(() => {
        const hash = window.location.hash;
        if (hash) {
            const params = new URLSearchParams(hash.substring(1));
            const access_token = params.get("access_token");
            const refresh_token = params.get("refresh_token");

            if (access_token && refresh_token) {
                dispatch(setAccessToken(access_token));
                dispatch(setRefreshToken(refresh_token));
            }

            window.history.replaceState(null, "", window.location.pathname + window.location.search);
        }

        navigate("/home", {replace: true});
    }, [dispatch, navigate]);

    return <Spinner size="huge"/>;
};