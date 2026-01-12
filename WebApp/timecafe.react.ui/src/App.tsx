// App.tsx
import {BrowserRouter, Routes, Route, Navigate} from 'react-router-dom';
import {lazy} from 'react';
import './App.css';
import "./api/interceptors.ts"
import { LoginPage } from './pages/LoginPage';


const AuthLayout = lazy(() => import('./layouts/AuthLayout/AuthLayuot').then(module => ({default: module.AuthLayout})));
const MainLayout = lazy(() => import('./layouts/MainLayout/MainLayuot').then(module => ({default: module.MainLayout})));

export default function App() {
    return (
        <div className="app_root">
            <BrowserRouter>
                <Routes>
                    <Route element={<AuthLayout/>}>
                        <Route path="/login" element={<LoginPage/>}/>
                        <Route path="/" element={<Navigate to="/login" replace/>}/>
                    </Route>

                    <Route element={<MainLayout/>}>

                    </Route>

                </Routes>
            </BrowserRouter>

        </div>
    )
}
