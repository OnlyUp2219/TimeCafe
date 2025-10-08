import './App.css'
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { LoginPage } from "./pages/LoginPage";
import { SignPage } from "./pages/SignPage";
import { Home } from "./pages/Home";
import { AuthLayout } from "./layouts/AuthLayout/AuthLayuot";
import { MainLayout } from "./layouts/MainLayout/MainLayuot";

export default function App() {
    return (

        <div className="app_root">
            <BrowserRouter>
                <Routes>
                    <Route path="*" element={<LoginPage />} />
                    <Route
                        path="sign"
                        element={
                            <AuthLayout>
                                <SignPage />
                            </AuthLayout>
                        }
                    />
                    <Route
                        path="home"
                        element={
                            <MainLayout>
                                <Home />
                            </MainLayout>
                        }
                    />
                </Routes>
            </BrowserRouter>
        </div>
    )
}
