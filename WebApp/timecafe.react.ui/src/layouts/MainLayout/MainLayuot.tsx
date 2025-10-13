import * as React from "react";
import "./MainLayout.css";
import {Header} from "../../components/Header/Header";
import {Sidebar} from "../../components/Sidebar/Sidebar";
import {Footer} from "../../components/Footer/Footer";
import {Outlet} from "react-router-dom";
import {useDispatch, useSelector} from "react-redux";
import {toggleSidebar} from "../../store/uiSlice.ts";
import type {RootState} from "../../store";

export const MainLayout: React.FC = () => {
    const dispatch = useDispatch();
    const isSidebarOpen = useSelector((state: RootState) => state.ui.isSideBarOpen);

    return (
        <div className="main-layout">
            <Header
                onMenuToggle={() => dispatch(toggleSidebar())}
                isSidebarOpen={isSidebarOpen}
            />

            <div className="main-layout__content">
                <Sidebar/>

                <main className="main-layout__main"><Outlet/></main>
            </div>

            <Footer/>
        </div>
    );
};
