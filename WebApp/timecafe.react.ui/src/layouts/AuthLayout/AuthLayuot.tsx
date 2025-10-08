import * as React from "react";
import "./AuthLayout.css";

export const AuthLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    return (
        <div className="auth-layout">
            <main className="auth-layout__main">{children}</main>
        </div>
    );
};
