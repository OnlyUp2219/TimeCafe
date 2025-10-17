import './App.css'
import {BrowserRouter, Routes, Route, Navigate} from "react-router-dom";
import {LoginPage} from "./pages/LoginPage";
import {SignPage} from "./pages/SignPage";
import {Home} from "./pages/Home";
import {AuthLayout} from "./layouts/AuthLayout/AuthLayuot";
import {MainLayout} from "./layouts/MainLayout/MainLayuot";
import {PersonalData} from "./pages/PersonalData";
import {ResetPassword} from "./pages/resetPassword/ResetPassword.tsx";
import {ResetPasswordEmail} from "./pages/resetPassword/ResetPasswordEmail.tsx";
import {PrivateRoute} from "./components/PrivateRoute/PrivateRoute.tsx";

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

                        <Route path="*" element={<LoginPage/>}/>
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
