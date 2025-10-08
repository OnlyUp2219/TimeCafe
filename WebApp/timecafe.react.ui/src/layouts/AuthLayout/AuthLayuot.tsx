import * as React from "react";
import "./AuthLayout.css";
import { Outlet } from "react-router-dom";

export const AuthLayout: React.FC = () => {
    return (
        <div className="auth-layout">
            <main className="auth-layout__main"><Outlet /></main>
        </div>
    );
};
