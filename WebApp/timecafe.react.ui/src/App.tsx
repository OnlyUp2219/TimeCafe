// App.tsx
import {BrowserRouter, Routes, Route, Navigate} from 'react-router-dom';
import {lazy} from 'react';
import './App.css';
import "./api/interceptors.ts"
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ResetPasswordPage } from './pages/ResetPasswordPage';
import { ConfirmResetPage } from './pages/ConfirmResetPage';


const AuthLayout = lazy(() => import('./layouts/AuthLayout/AuthLayuot').then(module => ({default: module.AuthLayout})));
const MainLayout = lazy(() => import('./layouts/MainLayout/MainLayuot').then(module => ({default: module.MainLayout})));

export default function App() {
    return (
        <div className="app_root">
            <BrowserRouter>
                <Routes>
                    <Route element={<AuthLayout/>}>
                        <Route path="/login" element={<LoginPage/>}/>
                        <Route path="/register" element={<RegisterPage/>}/>
                        <Route path="/reset-password" element={<ResetPasswordPage/>}/>
                        <Route path="/confirm-reset" element={<ConfirmResetPage/>}/>

                        <Route path="/" element={<Navigate to="/login" replace/>}/>
                    </Route>

                    <Route element={<MainLayout/>}>

                    </Route>

                </Routes>
            </BrowserRouter>

        </div>
    )
}
