import {Outlet} from "react-router-dom";
import type {FC} from "react";

export const AuthLayout: FC = () => {
    return (
        <div className="auth-layout">
            <main className="auth-layout__main"><Outlet/></main>
        </div>
    );
};
