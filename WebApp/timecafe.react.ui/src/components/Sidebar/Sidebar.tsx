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
import {useLocation, useNavigate} from "react-router-dom";
import {setSelectedNav, setSidebarOpen, toggleSidebar} from "../../store/uiSlice.ts";
import {useDispatch, useSelector} from "react-redux";
import type {RootState} from "../../store";
import {useEffect} from "react";

type DrawerType = Required<NavDrawerProps>["type"];


export const Sidebar: React.FC = () => {

    const dispatch = useDispatch();
    const isOpen = useSelector((state: RootState) => state.ui.isSideBarOpen);
    const location = useLocation();

    useEffect(() => {
        const navIdFromState = location.state?.navId;
        if (navIdFromState) {
            dispatch(setSelectedNav(navIdFromState));
        } else {
            const navItem = navItems.find(item => item.path === location.pathname);
            if (navItem) {
                dispatch(setSelectedNav(navItem.id));
            } else {
                dispatch(setSelectedNav("1"));
            }
        }
    }, [location]);


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

    const navItems = [
        {id: "1", label: "Главная", path: "/home"},
        {id: "2", label: "Персональные данные", path: "/personal-data"},
        {id: "3", label: "Тестовая", path: "/home"},
    ];


    return (
        <aside className="app-sidebar">
            <NavDrawer
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
                    {navItems.map((item) => (
                        <NavItem
                            value={item.id}
                            key={item.id}
                            onClick={() => {
                                if (selectedValue !== item.id) {
                                    dispatch(setSelectedNav(item.id));
                                    navigate(item.path, {state: {navId: item.id}});
                                }
                            }}
                        >
                            {item.label}
                        </NavItem>
                    ))}
                </NavDrawerBody>
            </NavDrawer>
        </aside>
    );
};
