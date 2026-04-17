import {
    Avatar,
    Button,
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
import {clearTokens} from "@store/authSlice";
import {setSelectedNav, setSidebarOpen, toggleSidebar} from "@store/uiSlice";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {type FC, type ReactElement, useCallback, useEffect, useMemo, useState} from "react";
import {useHasActiveVisitQuery} from "@store/api/venueApi";
import {selectUserId} from "@store/authSlice";
import {Home20Regular, Person20Regular, Clock20Regular, Money20Regular, Shield20Regular, Eye20Regular, SignOut20Regular} from "@fluentui/react-icons";
import {usePermissions} from "@hooks/usePermissions";
import {AdminPanelPermission} from "@shared/auth/permissions";

type DrawerType = Required<NavDrawerProps>["type"];

export const Sidebar: FC = () => {
    const dispatch = useAppDispatch();
    const isOpen = useAppSelector((state) => state.ui.isSideBarOpen);
    const userId = useAppSelector(selectUserId);
    const email = useAppSelector((state) => state.auth.email);
    const role = useAppSelector((state) => state.auth.role);
    const {has: hasPerm} = usePermissions();
    const canAccessAdmin = hasPerm(AdminPanelPermission);
    const location = useLocation();
    const navigate = useNavigate();
    const {data: hasActive} = useHasActiveVisitQuery(userId ?? "", {skip: !userId});
    const isAdminView = location.pathname.startsWith("/admin");

    const navItems = useMemo(() => {
        const visitNav = hasActive
            ? {id: "3", label: "Активный визит", path: "/visit/active", icon: <Clock20Regular />}
            : {id: "3", label: "Начать визит", path: "/visit/start", icon: <Clock20Regular />};

        return [
            {id: "1", label: "Главная", path: "/home", icon: <Home20Regular />},
            {id: "2", label: "Персональные данные", path: "/personal-data", icon: <Person20Regular />},
            visitNav,
            {id: "6", label: "Баланс и транзакции", path: "/billing", icon: <Money20Regular />},
        ] as {id: string; label: string; path: string; icon: ReactElement}[];
    }, [hasActive]);

    useEffect(() => {
        const navIdFromState = location.state?.navId;
        if (navIdFromState) {
            dispatch(setSelectedNav(navIdFromState));
            return;
        }

        const navItem = navItems.find(item => item.path === location.pathname);
        dispatch(setSelectedNav(navItem?.id ?? "1"));
    }, [location.pathname, location.state?.navId, dispatch, navItems]);

    const handleOpenChange = (open: boolean) => {
        dispatch(setSidebarOpen(open));
    };

    const [type, setType] = useState<DrawerType>("inline");

    const onMediaQueryChange = useCallback(({matches}: { matches: boolean }) => setType(matches ? "overlay" : "inline"), []);

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
        const item = navItems.find((navItem) => navItem.id === value);
        if (item) {
            navigate(item.path, {state: {navId: item.id}});
        }
    };

    const handleAdminView = () => {
        navigate("/admin/dashboard");
    };

    const handleLogout = () => {
        dispatch(clearTokens());
        navigate("/login", {replace: true});
    };

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
                            <Hamburger onClick={() => dispatch(toggleSidebar())} />
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
                        >
                            {item.label}
                        </NavItem>
                    ))}
                </NavDrawerBody>

                {canAccessAdmin && (
                    <div className="p-3 border-t border-neutral-200">
                        <div className="flex items-center gap-3 mb-3">
                            <Avatar name={email || role || "Admin"} size={isAdminView ? 32 : 28} />
                            <div className="min-w-0">
                                <div className="font-medium truncate">{isAdminView ? "Админ-режим" : "Режим клиента"}</div>
                                <div className="text-xs text-neutral-500 truncate">{email || role || "—"}</div>
                            </div>
                        </div>
                        <Button
                            appearance="subtle"
                            icon={isAdminView ? <Eye20Regular /> : <Shield20Regular />}
                            onClick={handleAdminView}
                            className="w-full"
                        >
                            Вид админа
                        </Button>
                        <Button
                            appearance="subtle"
                            icon={<SignOut20Regular />}
                            onClick={handleLogout}
                            className="w-full mt-2"
                        >
                            Выйти
                        </Button>
                    </div>
                )}
            </NavDrawer>
        </aside>
    );
};
