import {
    Hamburger,
    NavDrawer,
    NavDrawerBody,
    NavDrawerHeader,
    NavItem,
    Tooltip,
    useRestoreFocusSource,
    type NavDrawerProps,
    type OnNavItemSelectData,
} from "@fluentui/react-components";
import {useLocation, useNavigate} from "react-router-dom";
import {setSelectedNav, setSidebarOpen, toggleSidebar} from "@store/uiSlice";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {type FC, type ReactElement, useCallback, useEffect, useMemo, useState} from "react";
import {useHasActiveVisitQuery} from "@store/api/venueApi";
import {selectUserId} from "@store/authSlice";
import {Roles} from "@shared/auth/roles";
import {Home20Regular, Person20Regular, Clock20Regular, Money20Regular, People20Regular} from "@fluentui/react-icons";

type DrawerType = Required<NavDrawerProps>["type"];


export const Sidebar: FC = () => {

    const dispatch = useAppDispatch();
    const isOpen = useAppSelector((state) => state.ui.isSideBarOpen);
    const userId = useAppSelector(selectUserId);
    const role = useAppSelector((state) => state.auth.role);
    const isAdmin = role === Roles.Admin;
    const {data: hasActive} = useHasActiveVisitQuery(userId ?? "", {skip: !userId});
    const location = useLocation();

    const navItems = useMemo(() => {
        const visitNav = hasActive
            ? {id: "3", label: "Активный визит", path: "/visit/active", icon: <Clock20Regular />}
            : {id: "3", label: "Начать визит", path: "/visit/start", icon: <Clock20Regular />};

        return [
            {id: "1", label: "Главная", path: "/home", icon: <Home20Regular />},
            {id: "2", label: "Персональные данные", path: "/personal-data", icon: <Person20Regular />},
            visitNav,
            {id: "6", label: "Баланс и транзакции", path: "/billing", icon: <Money20Regular />},
            ...(isAdmin ? [{id: "7", label: "Пользователи", path: "/admin/users", icon: <People20Regular />}] : []),
        ] as {id: string; label: string; path: string; icon: ReactElement}[];
    }, [hasActive, isAdmin]);

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
        const match = globalThis.matchMedia("(max-width: 828px)");
        if (match.matches) {
            setType("overlay");
        }
        match.addEventListener("change", onMediaQueryChange);
        return () => match.removeEventListener("change", onMediaQueryChange);
    }, [onMediaQueryChange]);

    const restoreFocusSourceAttributes = useRestoreFocusSource();

    const selectedValue = useAppSelector((state) => state.ui.selectedNav);

    const handleItemSelect = (_: unknown, data: OnNavItemSelectData) => {
        const value = String(data.value);
        dispatch(setSelectedNav(value));


    };
    const navigate = useNavigate();

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
                            icon={item.icon}
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
