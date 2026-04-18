import type {FC} from "react";
import {useState} from "react";
import {Outlet} from "react-router-dom";
import {AdminSidebar} from "./AdminSidebar";
import {Hamburger} from "@fluentui/react-components";

export const AdminLayout: FC = () => {
    const [isMobileNavOpen, setIsMobileNavOpen] = useState(false);

    return (
        <div className="admin-layout flex flex-col h-screen">
            {/* Mobile Header, visible only on small screens */}
            <div className="md:hidden flex items-center p-3 border-b border-slate-200 bg-white/90 backdrop-blur z-50">
                <Hamburger 
                    onClick={() => setIsMobileNavOpen(!isMobileNavOpen)} 
                    aria-label="Toggle menu" 
                />
                <h1 className="ml-3 text-lg font-semibold text-slate-900">Admin Panel</h1>
            </div>

            <div className="flex flex-1 overflow-hidden">
                <AdminSidebar isOpen={isMobileNavOpen} onOpenChange={setIsMobileNavOpen} />
                <main className="admin-content flex-1 overflow-y-auto">
                    <Outlet />
                </main>
            </div>
        </div>
    );
};
