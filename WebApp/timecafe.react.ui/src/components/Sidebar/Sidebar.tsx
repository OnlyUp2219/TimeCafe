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
import {useLocation, useNavigate} from "react-router-dom";
import {setSelectedNav, setSidebarOpen, toggleSidebar} from "@store/uiSlice.ts";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {type FC, useCallback, useEffect, useMemo, useState} from "react";
import {useHasActiveVisitQuery} from "@store/api/venueApi";
import {selectUserId} from "@store/authSlice";

type DrawerType = Required<NavDrawerProps>["type"];


export const Sidebar: FC = () => {

    const dispatch = useAppDispatch();
    const isOpen = useAppSelector((state) => state.ui.isSideBarOpen);
    const userId = useAppSelector(selectUserId);
    const {data: hasActive} = useHasActiveVisitQuery(userId!, {skip: !userId});
    const location = useLocation();

    const navItems = useMemo(() => {
        const visitNav = hasActive
            ? {id: "3", label: "Активный визит", path: "/visit/active"}
            : {id: "3", label: "Начать визит", path: "/visit/start"};

        return [
            {id: "1", label: "Главная", path: "/home"},
            {id: "2", label: "Персональные данные", path: "/personal-data"},
            visitNav,
            {id: "6", label: "Баланс и транзакции", path: "/billing"},
        ];
    }, [hasActive]);

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
    }, [location, hasActive, dispatch, navItems]);


    const handleOpenChange = (open: boolean) => {
        dispatch(setSidebarOpen(open));
    };

    const [type, setType] = useState<DrawerType>("inline");

    const onMediaQueryChange = useCallback(
        ({matches}: { matches: boolean }) =>
            setType(matches ? "overlay" : "inline"),
        [setType]
    );

    useEffect(() => {
        const match = window.matchMedia("(max-width: 828px)");
        if (match.matches) {
            setType("overlay");
        }
        match.addEventListener("change", onMediaQueryChange);
        return () => match.removeEventListener("change", onMediaQueryChange);
    }, [onMediaQueryChange]);

    const restoreFocusSourceAttributes = useRestoreFocusSource();

    const selectedValue = useAppSelector((state) => state.ui.selectedNav);

    const handleItemSelect = (_: unknown, data: OnNavItemSelectData) => {
        const value = data.value as string;
        dispatch(setSelectedNav(value));


    };
    const navigate = useNavigate();

    const visitNav = hasActive
        ? {id: "3", label: "Активный визит", path: "/visit/active"}
        : {id: "3", label: "Начать визит", path: "/visit/start"};

    const navItems = [
        {id: "1", label: "Главная", path: "/home"},
        {id: "2", label: "Персональные данные", path: "/personal-data"},
        visitNav,
        {id: "6", label: "Баланс и транзакции", path: "/billing"},
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
                            data-testid={`sidebar-nav-${item.id}`}
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
