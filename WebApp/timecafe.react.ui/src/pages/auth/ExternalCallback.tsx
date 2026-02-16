import {useDispatch} from "react-redux";
import {useNavigate} from "react-router-dom";
import {clearTokens, setAccessToken, setEmail, setEmailConfirmed, setRole, setUserId} from "@store/authSlice.ts";
import {Spinner} from "@fluentui/react-components";
import React, {useEffect} from "react";
import {getJwtInfo} from "@shared/auth/jwt";

export const ExternalCallback = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const redirectedRef = React.useRef(false);

    useEffect(() => {
        if (redirectedRef.current) return;

        const hash = window.location.hash;
        if (hash) {
            const params = new URLSearchParams(hash.substring(1));
            const access_token = params.get("access_token");
            const email_confirmed = params.get("emailConfirmed");

            if (access_token) {
                dispatch(clearTokens());
                dispatch(setAccessToken(access_token));
                const info = getJwtInfo(access_token);
                if (info.userId) dispatch(setUserId(info.userId));
                if (info.role) dispatch(setRole(info.role));
                if (info.email) dispatch(setEmail(info.email));

                const confirmed = email_confirmed === "true" || email_confirmed === "1";
                dispatch(setEmailConfirmed(confirmed));
                window.history.replaceState(null, "", window.location.pathname + window.location.search);
                redirectedRef.current = true;
                navigate("/home", {replace: true});
            }
        }
    }, [dispatch, navigate]);

    return <Spinner size="huge"/>;
};