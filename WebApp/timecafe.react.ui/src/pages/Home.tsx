import * as React from 'react';
import "./Home.css"
import {Button, Spinner} from "@fluentui/react-components";
import {useEffect} from "react";
import {refreshToken as refreshTokenApi} from "../api/auth.ts";
import axios from "axios";
import {useNavigate} from "react-router-dom";


export const Home = () => {
    const navigate = useNavigate();

    const [accessToken, setAccessToken] = React.useState<string | null>(null);
    const [refreshToken, setRefreshToken] = React.useState<string | null>(null);
    const [refreshResult, setRefreshResult] = React.useState<string | null>(null);
    const [protectedResult, setProtectedResult] = React.useState<string | null>(null);
    const [checkingAuth, setCheckingAuth] = React.useState(true);

    useEffect(() => {
        const a = localStorage.getItem("accessToken");
        const r = localStorage.getItem("refreshToken");
        if (!a) {
            navigate("/login");
        } else {
            setAccessToken(a);
            setRefreshToken(r);
        }
        setCheckingAuth(false);
    }, [navigate]);

    const apiBase = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7057";

    const handleRefresh = async () => {
        setRefreshResult(null);
        try {
            await refreshTokenApi();
            setAccessToken(localStorage.getItem("accessToken"));
            setRefreshResult("OK");
        } catch (e: any) {
            setRefreshResult(String(e?.message ?? e));
        }
    };

    const handleClearAccessJwt = () => {
        localStorage.removeItem("accessToken");
        setAccessToken(null);
        setProtectedResult(null);
    };

    const handleClearRefreshJwt = () => {
        localStorage.removeItem("refreshToken");
        setRefreshToken(null);
        setProtectedResult(null);
    };

    const callProtected = async () => {
        setProtectedResult(null);
        try {
            const token = localStorage.getItem("accessToken");
            const res = await axios.get(`${apiBase}/protected-test`, {
                headers: {Authorization: `Bearer ${token}`},
            });
            setAccessToken(localStorage.getItem("accessToken"));
            setProtectedResult(res.data);
        } catch (e: any) {
            setProtectedResult(String(e?.message ?? e));
        }
    };

    if (checkingAuth) {
        return <Spinner size={"huge"}/>;
    }

    return (
        <div className="home_root">
            <h1>Добро пожаловать!</h1>
            <p>На главную страницу</p>

            <div style={{marginTop: 20}}>
                <div>
                    <b>Access token:</b>
                </div>
                <pre style={{whiteSpace: "pre-wrap", wordBreak: "break-all"}}>
                    {accessToken ?? "<empty>"}
                </pre>
                <div>
                    <b>Refresh token:</b>
                </div>
                <pre style={{whiteSpace: "pre-wrap", wordBreak: "break-all"}}>
                    {refreshToken ?? "<empty>"}
                </pre>


                <Button onClick={handleRefresh}>Refresh token</Button>
                <Button onClick={handleClearAccessJwt}>Clear JWT Access</Button>
                <Button onClick={handleClearRefreshJwt}>Clear JWT Refresh</Button>
                <Button onClick={callProtected}>Call protected endpoint</Button>

                {refreshResult && <div>Refresh: {refreshResult}</div>}
                {protectedResult && <div>Protected result: {protectedResult}</div>}
            </div>
        </div>
    );
};