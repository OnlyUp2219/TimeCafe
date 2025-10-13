import * as React from "react";
import {
    Hamburger,
    NavDrawer,
    NavDrawerBody,
    NavDrawerHeader,
    NavItem,
    Tooltip,
    useRestoreFocusSource,
    type NavDrawerProps,
} from "@fluentui/react-components";
import type {OnNavItemSelectData} from "@fluentui/react-components";
import "./Sidebar.css";
import {useNavigate} from "react-router-dom";
import {setSelectedNav, setSidebarOpen, toggleSidebar} from "../../store/uiSlice.ts";
import {useDispatch, useSelector} from "react-redux";
import type {RootState} from "../../store";

type DrawerType = Required<NavDrawerProps>["type"];


export const Sidebar: React.FC = () => {

    const dispatch = useDispatch();
    const isOpen = useSelector((state: RootState) => state.ui.isSideBarOpen);

    const handleOpenChange = (open: boolean) => {
        dispatch(setSidebarOpen(open));
    };

    const [type, setType] = React.useState<DrawerType>("inline");

    const onMediaQueryChange = React.useCallback(
        ({matches}: { matches: boolean }) =>
            setType(matches ? "overlay" : "inline"),
        [setType]
    );

    React.useEffect(() => {
        const match = window.matchMedia("(max-width: 720px)");
        if (match.matches) {
            setType("overlay");
        }
        match.addEventListener("change", onMediaQueryChange);
        return () => match.removeEventListener("change", onMediaQueryChange);
    }, [onMediaQueryChange]);

    const restoreFocusSourceAttributes = useRestoreFocusSource();

    const selectedValue = useSelector((state: RootState) => state.ui.selectedNav);

    const handleItemSelect = (_: unknown, data: OnNavItemSelectData) => {
        const value = data.value as string;
        dispatch(setSelectedNav(value));
    };
    const navigate = useNavigate();

    return (
        <aside className="app-sidebar">
            <NavDrawer
                defaultSelectedValue="1"
                onNavItemSelect={handleItemSelect}
                selectedValue={selectedValue}
                type={type}
                {...restoreFocusSourceAttributes}
                separator
                position="start"
                open={isOpen}
                onOpenChange={(_, {open}) => handleOpenChange(open)}
                className="sidebar"
            >
                <NavDrawerHeader>
                    <div>
                        <Tooltip content="Close Navigation" relationship="label">
                            <Hamburger onClick={() => dispatch(toggleSidebar())}/>
                        </Tooltip>
                        Основное
                    </div>
                </NavDrawerHeader>

                <NavDrawerBody>
                    <NavItem value="1" onClick={() => navigate("home")}>Главная</NavItem>
                    <NavItem value="2" onClick={() => navigate("personal-data")}>Персональные данные</NavItem>
                    <NavItem value="3" onClick={() => navigate("home")}>Тестовоя</NavItem>
                </NavDrawerBody>
            </NavDrawer>
        </aside>
    );
};
