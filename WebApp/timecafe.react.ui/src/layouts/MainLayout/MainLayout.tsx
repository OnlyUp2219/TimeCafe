import {Header} from "@components/Header/Header";
import {Sidebar} from "@components/Sidebar/Sidebar";
import {Footer} from "@components/Footer/Footer";
import {Outlet} from "react-router-dom";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {toggleSidebar} from "@store/uiSlice";
import type {FC} from "react";
import {ProfileCompletionGate} from "@layouts/MainLayout/ProfileCompletionGate";

export const MainLayout: FC = () => {
    const dispatch = useAppDispatch();
    const isSidebarOpen = useAppSelector((state) => state.ui.isSideBarOpen);

    return (
        <div className="main-layout min-h-[100dvh]" style={{
            display: "grid",
            gridTemplateColumns: "auto 1fr",
            gridTemplateRows: "auto 1fr auto",
            gridTemplateAreas: `
                "header header"
                "sidebar main"
                "footer footer"
            `
        }}>
            <div style={{ gridArea: "header" }}>
                <Header
                    onMenuToggle={() => dispatch(toggleSidebar())}
                    isSidebarOpen={isSidebarOpen}
                />
                <ProfileCompletionGate />
            </div>

            <div style={{ gridArea: "sidebar" }} className="bg-(--colorNeutralBackground1) border-r border-(--colorNeutralStroke2)">
                <Sidebar />
            </div>

            <main style={{ gridArea: "main" }} className="flex flex-col min-w-0">
                <Outlet />
            </main>

            <div style={{ gridArea: "footer" }}>
                <Footer />
            </div>
        </div>
    );
};
