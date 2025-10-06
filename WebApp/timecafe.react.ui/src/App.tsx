import './App.css'
import { BrowserRouter, Routes, Route } from "react-router-dom";
//import { StartPage } from "./pages/StartPage";
import { LoginPage } from "./pages/LoginPage";
import { SignPage } from "./pages/SignPage";
import { Home } from "./pages/home";

export default function App() {
    return (

        <div className="app_root">
            <BrowserRouter>
                <Routes>
                    <Route path="*" element={<LoginPage />} />
                    <Route path="sign" element={<SignPage />} />
                    <Route path="home" element={<Home />} />
                </Routes>
            </BrowserRouter>
        </div>
    )
}
