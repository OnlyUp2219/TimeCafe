// App.tsx
import {BrowserRouter, Routes, Route, useNavigate} from "react-router-dom";
import {useEffect} from "react";

import "./App.css";
import {LoginPage} from '@pages/auth/LoginPage';
import {RegisterPage} from '@pages/auth/RegisterPage';
import {ResetPasswordPage} from '@pages/auth/ResetPasswordPage';
import {ConfirmResetPage} from '@pages/auth/ConfirmResetPage';
import {LandingPage} from '@pages/public/LandingPage';
import {HomePage} from '@pages/home/HomePage';
import {PersonalDataPage} from "@pages/profile/PersonalDataPage";
import {TariffSelectionPage} from "@pages/visits/TariffSelectionPage";
import {ActiveVisitPage} from "@pages/visits/ActiveVisitPage";
import {AuthLayout} from '@layouts/AuthLayout/AuthLayuot';
import {MainLayout} from '@layouts/MainLayout/MainLayuot';
import {BillingPage} from '@pages/billing/BillingPage.tsx'
import {useProgressToast} from "@components/ToastProgress/ToastProgress";
import {configureHttpClient} from "@api/httpClient";
import {store} from "@store";
import {clearTokens, setAccessToken, setEmail, setRole, setUserId} from "@store/authSlice";
import {getJwtInfo} from "@shared/auth/jwt";
import {authApi} from "@api/auth/authApi";
import {ExternalCallback} from "@pages/auth/ExternalCallback";
import {EmailPendingPage} from "@pages/auth/EmailPendingPage";
import {ConfirmEmailPage} from "@pages/auth/ConfirmEmailPage";
import {PrivateRoute} from "@components/PrivateRoute/PrivateRoute";
import {JwtCrossServiceTestPage} from "@pages/dev/JwtCrossServiceTestPage";

const AppRoutes = () => {
    const navigate = useNavigate();
    const {showToast, ToasterElement} = useProgressToast();

    useEffect(() => {
        const setToken = (token: string | null) => {
            if (!token) {
                store.dispatch(clearTokens());
                return;
            }
            store.dispatch(setAccessToken(token));
            const info = getJwtInfo(token);
            if (info.userId) store.dispatch(setUserId(info.userId));
            if (info.role) store.dispatch(setRole(info.role));
            if (info.email) store.dispatch(setEmail(info.email));
        };

        configureHttpClient({
            getAccessToken: () => store.getState().auth.accessToken || null,
            setAccessToken: setToken,
            refreshAccessToken: authApi.tryRefreshAccessToken,
            handlers: {
                onUnauthorized: () => {
                    setToken(null);
                    showToast("Сессия истекла. Войдите снова.", "error", "Авторизация");
                    navigate("/login");
                },
                onForbidden: () => {
                    showToast("Недостаточно прав для выполнения действия.", "error", "Доступ запрещён");
                },
            },
        });
    }, [navigate, showToast]);

    return (
        <>
            {ToasterElement}
            <Routes>
                <Route path="/" element={<LandingPage/>}/>

                <Route element={<AuthLayout/>}>
                    <Route path="/login" element={<LoginPage/>}/>
                    <Route path="/register" element={<RegisterPage/>}/>
                    <Route path="/reset-password" element={<ResetPasswordPage/>}/>
                    <Route path="/confirm-reset" element={<ConfirmResetPage/>}/>
                    <Route path="/external-callback" element={<ExternalCallback/>}/>
                    <Route path="/email-pending" element={<EmailPendingPage/>}/>
                    <Route path="/confirm-email" element={<ConfirmEmailPage/>}/>
                </Route>

                <Route element={<PrivateRoute><MainLayout/></PrivateRoute>}>
                    <Route path="/home" element={<HomePage/>}/>
                    <Route path="/personal-data" element={<PersonalDataPage/>}/>
                    <Route path="/visit/start" element={<TariffSelectionPage/>}/>
                    <Route path="/visit/active" element={<ActiveVisitPage/>}/>
                    <Route path="/billing" element={<BillingPage/>}/>
                </Route>
                <Route path="/dev/jwt-test" element={<JwtCrossServiceTestPage/>}/>

            </Routes>
        </>
    );
};

export default function App() {
    return (
        <div className="app_root">
            <BrowserRouter>
                <AppRoutes/>
            </BrowserRouter>
        </div>
    )
}
