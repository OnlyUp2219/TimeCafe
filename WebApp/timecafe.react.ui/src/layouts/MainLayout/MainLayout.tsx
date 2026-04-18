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
        <div className="main-layout">
            <Header
                onMenuToggle={() => dispatch(toggleSidebar())}
                isSidebarOpen={isSidebarOpen}
            />

            <ProfileCompletionGate />

                
            <main className="main-layout__main flex flex-1 min-w-0">
                <Sidebar />
                <div className="flex-1 min-w-0 flex flex-col">
                    <Outlet/>
                </div>
            </main>

            <Footer/>
        </div>
    );
};
