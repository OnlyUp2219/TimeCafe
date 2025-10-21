// App.tsx
import {BrowserRouter, Routes, Route, Navigate} from 'react-router-dom';
import {lazy} from 'react';
import './App.css';

const LoginPage = lazy(() => import('./pages/LoginPage').then(module => ({default: module.LoginPage})));
const SignPage = lazy(() => import('./pages/SignPage').then(module => ({default: module.SignPage})));
const Home = lazy(() => import('./pages/Home').then(module => ({default: module.Home})));
const AuthLayout = lazy(() => import('./layouts/AuthLayout/AuthLayuot').then(module => ({default: module.AuthLayout})));
const MainLayout = lazy(() => import('./layouts/MainLayout/MainLayuot').then(module => ({default: module.MainLayout})));
const PersonalData = lazy(() => import('./pages/PersonalData').then(module => ({default: module.PersonalData})));
const ResetPassword = lazy(() => import('./pages/resetPassword/ResetPassword').then(module => ({default: module.ResetPassword})));
const ResetPasswordEmail = lazy(() => import('./pages/resetPassword/ResetPasswordEmail').then(module => ({default: module.ResetPasswordEmail})));
const PrivateRoute = lazy(() => import('./components/PrivateRoute/PrivateRoute').then(module => ({default: module.PrivateRoute})));
const ExternalCallback = lazy(() => import('./pages/ExternalCallback').then(module => ({default: module.ExternalCallback})));

export default function App() {
    return (
        <div className="app_root">
            <BrowserRouter>
                <Routes>
                    <Route element={<AuthLayout/>}>
                        <Route path="sign" element={<SignPage/>}/>
                        <Route path="login" element={<LoginPage/>}/>
                        <Route path={"resetPassword"} element={<ResetPassword/>}/>
                        <Route path={"resetPasswordEmail"} element={<ResetPasswordEmail/>}/>
                        <Route path="external-callback" element={<ExternalCallback/>}/>
                    </Route>

                    <Route element={<MainLayout/>}>
                        <Route
                            path="home"
                            element={
                                <PrivateRoute>
                                    <Home/>
                                </PrivateRoute>
                            }
                        />
                        <Route
                            path="personal-data"
                            element={
                                <PrivateRoute>
                                    <PersonalData/>
                                </PrivateRoute>
                            }
                        />
                        <Route path="/" element={<Navigate to="/home" replace/>}/>
                        <Route path="*" element={<Navigate to="/home" replace/>}/>
                    </Route>

                </Routes>
            </BrowserRouter>

        </div>
    )
}
