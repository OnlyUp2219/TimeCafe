
import * as React from "react";
import "./MainLayout.css";
import { Header } from "../../components/Header/Header";
import { Sidebar } from "../../components/Sidebar/Sidebar";
import { Footer } from "../../components/Footer/Footer";

export const MainLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [isSidebarOpen, setIsSidebarOpen] = React.useState(true);

    return (
        <div className="main-layout">
            <Header
                onMenuToggle={() => setIsSidebarOpen(!isSidebarOpen)}
                isSidebarOpen={isSidebarOpen}
            />

            <div className="main-layout__content">
                <Sidebar
                    isOpen={isSidebarOpen}
                    onOpenChange={(open: boolean) => setIsSidebarOpen(open)}
                />

                <main className="main-layout__main">{children}</main>
            </div>

            <Footer />
        </div>
    );
};
