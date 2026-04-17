import {BrowserRouter, Routes, Route, Navigate, useNavigate} from "react-router-dom";
import {useEffect} from "react";

import "./App.css";
import {LoginPage} from "@pages/auth/LoginPage";
import {RegisterPage} from "@pages/auth/RegisterPage";
import {ResetPasswordPage} from "@pages/auth/ResetPasswordPage";
import {ConfirmResetPage} from "@pages/auth/ConfirmResetPage";
import {LandingPage} from "@pages/public/LandingPage";
import {HomePage} from "@pages/home/HomePage";
import {PersonalDataPage} from "@pages/profile/PersonalDataPage";
import {TariffSelectionPage} from "@pages/visits/TariffSelectionPage";
import {ActiveVisitPage} from "@pages/visits/ActiveVisitPage";
import {AuthLayout} from "@layouts/AuthLayout/AuthLayout";
import {MainLayout} from "@layouts/MainLayout/MainLayout";
import {BillingPage} from "@pages/billing/BillingPage";
import {useProgressToast} from "@components/ToastProgress/ToastProgress";
import {configureHttpClient} from "@api/httpClient";
import {store} from "@store";
import {clearTokens, setAccessToken, setEmail, setRole, setUserId} from "@store/authSlice";
import {getJwtInfo} from "@shared/auth/jwt";
import {tryRefreshAccessToken} from "@shared/auth/refreshToken";
import {ExternalCallback} from "@pages/auth/ExternalCallback";
import {EmailPendingPage} from "@pages/auth/EmailPendingPage";
import {ConfirmEmailPage} from "@pages/auth/ConfirmEmailPage";
import {PrivateRoute} from "@components/PrivateRoute/PrivateRoute";
import {JwtCrossServiceTestPage} from "@pages/dev/JwtCrossServiceTestPage";
import {AdminRoute} from "@components/AdminRoute/AdminRoute";
import {UsersListPage} from "@pages/admin/UsersListPage";
import {UserDetailPage} from "@pages/admin/UserDetailPage";
import {TariffsPage} from "@pages/admin/TariffsPage";
import {DashboardPage} from "@pages/admin/DashboardPage";
import {PromotionsPage} from "@pages/admin/PromotionsPage";
import {ThemesPage} from "@pages/admin/ThemesPage";
import {VisitsPage} from "@pages/admin/VisitsPage";
import {TransactionsPage} from "@pages/admin/TransactionsPage";
import {PaymentsPage} from "@pages/admin/PaymentsPage";
import {BalancesPage} from "@pages/admin/BalancesPage";
import {RolesPage} from "@pages/admin/RolesPage";
import {RoleClaimsPage} from "@pages/admin/RoleClaimsPage";
import {UserRolesPage} from "@pages/admin/UserRolesPage";
import {VisitDetailPage} from "@pages/admin/VisitDetailPage";
import {AdminLayout} from "@layouts/AdminLayout/AdminLayout";

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
            refreshAccessToken: tryRefreshAccessToken,
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
                <Route element={<AdminRoute><AdminLayout/></AdminRoute>}>
                    <Route index path="/admin" element={<Navigate to="/admin/dashboard" replace />}/>
                    <Route path="/admin/dashboard" element={<DashboardPage/>}/>
                    <Route path="/admin/users" element={<UsersListPage/>}/>
                    <Route path="/admin/users/:id" element={<UserDetailPage/>}/>
                    <Route path="/admin/tariffs" element={<TariffsPage/>}/>
                    <Route path="/admin/promotions" element={<PromotionsPage/>}/>
                    <Route path="/admin/themes" element={<ThemesPage/>}/>
                    <Route path="/admin/visits" element={<VisitsPage/>}/>
                    <Route path="/admin/visits/:id" element={<VisitDetailPage/>}/>
                    <Route path="/admin/transactions" element={<TransactionsPage/>}/>
                    <Route path="/admin/payments" element={<PaymentsPage/>}/>
                    <Route path="/admin/balances" element={<BalancesPage/>}/>
                    <Route path="/admin/roles" element={<RolesPage/>}/>
                    <Route path="/admin/roles/:roleName/claims" element={<RoleClaimsPage/>}/>
                    <Route path="/admin/users/:id/roles" element={<UserRolesPage/>}/>
                </Route>
                <Route path="/dev/jwt-test" element={<JwtCrossServiceTestPage/>}/>

            </Routes>
        </>
    );
};

import {ComponentSizeProvider} from "@hooks/useComponentSize";

export default function App() {
    return (
        <ComponentSizeProvider defaultPreset="large">
            <div className="app_root">
                <BrowserRouter>
                    <AppRoutes/>
                </BrowserRouter>
            </div>
        </ComponentSizeProvider>
    )
}
