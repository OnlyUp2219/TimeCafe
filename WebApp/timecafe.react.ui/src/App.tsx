// App.tsx
import {BrowserRouter, Routes, Route} from 'react-router-dom';

import './App.css';
import "./api/interceptors.ts"
import {LoginPage} from './pages/auth/LoginPage';
import {RegisterPage} from './pages/auth/RegisterPage';
import {ResetPasswordPage} from './pages/auth/ResetPasswordPage';
import {ConfirmResetPage} from './pages/auth/ConfirmResetPage';
import {LandingPage} from './pages/public/LandingPage';
import {HomePage} from './pages/home/HomePage';
import {PersonalDataPage} from "./pages/profile/PersonalDataPage";
import {AuthLayout} from './layouts/AuthLayout/AuthLayuot';
import {MainLayout} from './layouts/MainLayout/MainLayuot';


export default function App() {
    return (
        <div className="app_root">
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<LandingPage/>}/>

                    <Route element={<AuthLayout/>}>
                        <Route path="/login" element={<LoginPage/>}/>
                        <Route path="/register" element={<RegisterPage/>}/>
                        <Route path="/reset-password" element={<ResetPasswordPage/>}/>
                        <Route path="/confirm-reset" element={<ConfirmResetPage/>}/>
                    </Route>

                    <Route element={<MainLayout/>}>
                        <Route path="/home" element={<HomePage/>}/>
                        <Route path="/personal-data" element={<PersonalDataPage/>}/>
                        <Route path="/login-test" element={<LoginPage/>}/>

                    </Route>

                </Routes>
            </BrowserRouter>

        </div>
    )
}
