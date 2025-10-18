import * as React from 'react';
import "./Home.css"
import {Button, type ToastIntent} from "@fluentui/react-components";
import {useEffect} from "react";
import {refreshToken as refreshTokenApi} from "../api/auth.ts";
import axios from "axios";
import {useNavigate} from "react-router-dom";
import {useProgressToast} from "../components/ToastProgress/ToastProgress.tsx";


export const Home = () => {
    const navigate = useNavigate();

    const [accessToken, setAccessToken] = React.useState<string | null>(null);
    const [refreshToken, setRefreshToken] = React.useState<string | null>(null);
    const [refreshResult, setRefreshResult] = React.useState<string | null>(null);
    const [protectedResult, setProtectedResult] = React.useState<string | null>(null);
    const [userRole, setUserRole] = React.useState<string | null>(null);
    const [functionResult, setFunctionResult] = React.useState<string | null>(null);

    useEffect(() => {
        const a = localStorage.getItem("accessToken");
        const r = localStorage.getItem("refreshToken");

        if (!a) {
            navigate("/login");
        } else {
            setAccessToken(a);
            setRefreshToken(r);
            setUserRole(getRoleFromToken(a));
        }
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

    const {showToast, ToasterElement} = useProgressToast();

    const callPublicFunction = async () => {
        setFunctionResult(null);
        try {
            const res = await axios.get(`${apiBase}/Functions/public-function`, {
                headers: {"Content-Type": "application/json"},
            });
            setFunctionResult(res.data);
        } catch (e: any) {
            showToast(`Ошибка: ${e?.message ?? e}`, "error");
        }
    };

    const callAdminFunction = async () => {
        setFunctionResult(null);
        if (userRole !== "admin") {
            showToast("У вас нет прав для выполнения этой функции", "error");
            return;
        }
        try {
            const res = await axios.get(`${apiBase}/Functions/admin-function`, {
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${accessToken}`
                },
            });
            setFunctionResult(res.data);
        } catch (e: any) {
            showToast(`Ошибка: ${e?.message ?? e}`, "error");
        }
    };

    const handleClick = () => {
        const intents: ToastIntent[] = ["success", "error", "warning", "info"];
        const intent = intents[Math.floor(Math.random() * intents.length)];
        showToast(`Случайный toast с intent: ${intent}`, intent);
    };


    const getRoleFromToken = (token: string | null) => {
        if (!token) return "client";
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ?? "client";
        } catch {
            return "client";
        }
    };

    return (
        <div className="home_root flex flex-col gap-[16px]">
            {ToasterElement}

            <div>
                <h1>Добро пожаловать!</h1>
                <p>На главную страницу</p>
                <h2>Ваша роль: {userRole}</h2>
            </div>

            <div className="flex flex-col">
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

                <div className="flex flex-wrap gap-[12px]">
                    <Button onClick={handleRefresh}>Refresh token</Button>
                    <Button onClick={handleClearAccessJwt}>Clear JWT Access</Button>
                    <Button onClick={handleClearRefreshJwt}>Clear JWT Refresh</Button>
                    <Button onClick={callProtected}>Call protected endpoint</Button>
                </div>

                {refreshResult && <div>Refresh: {refreshResult}</div>}
                {protectedResult && <div>Protected result: {protectedResult}</div>}
            </div>

            <div>
                <div className="flex gap-[16px] justify-around">
                    <Button onClick={callPublicFunction}>Вызвать общую функцию</Button>
                    <Button onClick={callAdminFunction}>Вызвать функцию админа</Button>
                </div>
                {functionResult && (
                    <div>
                        <h3>Результат выполнения функции:</h3>
                        <p>{functionResult}</p>
                    </div>
                )}
            </div>

            <Button onClick={handleClick}>Показать случайный toast</Button>

        </div>
    );
};