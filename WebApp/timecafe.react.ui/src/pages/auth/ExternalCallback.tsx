import {useDispatch} from "react-redux";
import {useNavigate} from "react-router-dom";
import {setAccessToken, setEmailConfirmed} from "../../store/authSlice.ts";
import {Spinner} from "@fluentui/react-components";
import React, {useEffect} from "react";

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
                dispatch(setAccessToken(access_token));
                dispatch(setEmailConfirmed(email_confirmed));
                window.history.replaceState(null, "", window.location.pathname + window.location.search);
                redirectedRef.current = true;
                navigate("/home", {replace: true});
            }
        }
    }, [dispatch, navigate]);

    return <Spinner size="huge"/>;
};