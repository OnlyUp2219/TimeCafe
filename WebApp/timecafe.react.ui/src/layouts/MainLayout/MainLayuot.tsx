import {Header} from "@components/Header/Header";
import {Sidebar} from "@components/Sidebar/Sidebar";
import {Footer} from "@components/Footer/Footer";
import {Outlet} from "react-router-dom";
import {useDispatch, useSelector} from "react-redux";
import {toggleSidebar} from "@store/uiSlice.ts";
import type {RootState} from "@store";
import type {FC} from "react";
import {ProfileCompletionGate} from "./ProfileCompletionGate";

export const MainLayout: FC = () => {
    const dispatch = useDispatch();
    const isSidebarOpen = useSelector((state: RootState) => state.ui.isSideBarOpen);

    return (
        <div className="main-layout">
            <Header
                onMenuToggle={() => dispatch(toggleSidebar())}
                isSidebarOpen={isSidebarOpen}
            />

            <ProfileCompletionGate />

            <div className="main-layout__content">
                <Sidebar/>

                <main className="main-layout__main"><Outlet/></main>
            </div>

            <Footer/>
        </div>
    );
};
