import './App.css'
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { LoginPage } from "./pages/LoginPage";
import { SignPage } from "./pages/SignPage";
import { Home } from "./pages/Home";
import { AuthLayout } from "./layouts/AuthLayout/AuthLayuot";
import { MainLayout } from "./layouts/MainLayout/MainLayuot";
import { PersonalData } from "./pages/PersonalData";

export default function App() {
    return (

        <div className="app_root">
            <BrowserRouter>
                <Routes>
                    <Route element={<AuthLayout />}>
                        <Route path="sign" element={<SignPage />} />
                        <Route path="login" element={<LoginPage />} />
                        <Route path="*" element={<LoginPage />} />
                    </Route>

                    <Route element={<MainLayout />}>
                        <Route path="home" element={<Home />} />
                        <Route path="personal-data" element={<PersonalData />} />
                    </Route>
                    
                </Routes>
            </BrowserRouter>

        </div>
    )
}
