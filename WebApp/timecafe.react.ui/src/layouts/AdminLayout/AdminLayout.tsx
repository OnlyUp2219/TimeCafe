import type {FC} from "react";
import {Outlet} from "react-router-dom";
import {AdminSidebar} from "./AdminSidebar";

export const AdminLayout: FC = () => {
    return (
        <div className="admin-layout">
            <AdminSidebar />
            <main className="admin-content">
                <Outlet />
            </main>
        </div>
    );
};
