import { type FC, useEffect, useMemo } from "react";
import { Avatar, Body1, Button, Caption1, Tooltip } from "@fluentui/react-components";
import { useLocation, useNavigate } from "react-router-dom";
import { clearTokens, selectUserId } from "@store/authSlice";
import { useAppDispatch, useAppSelector } from "@store/hooks";
import { setSelectedNav, setSidebarOpen, setSidebarCollapsed } from "@store/uiSlice";
import { useHasActiveVisitQuery } from "@store/api/venueApi";
import { Home20Regular, Person20Regular, Clock20Regular, Money20Regular, Eye20Regular, SignOut20Regular } from "@fluentui/react-icons";
import { usePermissions } from "@hooks/usePermissions";
import { AdminPanelPermission } from "@shared/auth/permissions";
import { BaseSidebar, type NavSectionType, type NavItemType } from "./BaseSidebar";

export const Sidebar: FC = () => {
    const dispatch = useAppDispatch();
    const isOpen = useAppSelector((state) => state.ui.isSideBarOpen);
    const userId = useAppSelector(selectUserId);
    const email = useAppSelector((state) => state.auth.email);
    const role = useAppSelector((state) => state.auth.role);
    const location = useLocation();
    const navigate = useNavigate();
    const { data: hasActive } = useHasActiveVisitQuery(userId ?? "", { skip: !userId });
    const { has: hasPerm } = usePermissions();
    const canAccessAdmin = hasPerm(AdminPanelPermission);

    const sections: NavSectionType[] = useMemo(() => {
        const visitItem = hasActive
            ? { id: "visit", label: "Активный визит", path: "/visit/active", icon: <Clock20Regular /> }
            : { id: "visit", label: "Начать визит", path: "/visit/start", icon: <Clock20Regular /> };

        return [
            {
                title: "Основное",
                items: [
                    { id: "home", label: "Главная", path: "/home", icon: <Home20Regular /> },
                    { id: "profile", label: "Персональные данные", path: "/personal-data", icon: <Person20Regular /> },
                    visitItem,
                    { id: "billing", label: "Баланс и транзакции", path: "/billing", icon: <Money20Regular /> },
                ],
            },
        ];
    }, [hasActive]);

    const navItems = useMemo(() => sections.flatMap((section) => section.items), [sections]);
    const bottomNav: NavItemType[] = useMemo(() => navItems.slice(0, 4), [navItems]);

    const selectedValue = useMemo(() => {
        const navItem = navItems.find((item) => location.pathname.startsWith(item.path));
        return navItem?.id ?? "home";
    }, [location.pathname, navItems]);

    useEffect(() => {
        dispatch(setSelectedNav(selectedValue));
    }, [dispatch, selectedValue]);

    const handleAdminView = () => {
        navigate("/admin/dashboard");
        dispatch(setSidebarOpen(false));
    };

    const handleLogout = () => {
        dispatch(clearTokens());
        navigate("/login", { replace: true });
    };

    const renderBottom = (compact: boolean) => (
        <>
            {compact ? (
                <div className="user-sidebar__bottom-compact">
                    <Avatar name={email || role || "User"} size={28} />
                    {canAccessAdmin && (
                        <Tooltip content="Вид админа" relationship="label" positioning="after">
                            <Button appearance="subtle" size="small" icon={<Eye20Regular />} onClick={handleAdminView} />
                        </Tooltip>
                    )}
                    <Tooltip content="Выйти" relationship="label" positioning="after">
                        <Button appearance="subtle" size="small" icon={<SignOut20Regular />} onClick={handleLogout} />
                    </Tooltip>
                </div>
            ) : (
                <>
                    <div className="user-sidebar__bottom-user">
                        <Avatar name={email || role || "User"} size={32} />
                        <div className="min-w-0">
                            <Body1 truncate wrap={false} block>Пользователь</Body1>
                            <Caption1 truncate wrap={false} block>{email || role || "—"}</Caption1>
                        </div>
                    </div>
                    <div className="user-sidebar__bottom-actions">
                        {canAccessAdmin && (
                            <Button appearance="subtle" size="small" icon={<Eye20Regular />} onClick={handleAdminView}>Вид админа</Button>
                        )}
                        <Button appearance="subtle" size="small" icon={<SignOut20Regular />} onClick={handleLogout}>Выйти</Button>
                    </div>
                </>
            )}
        </>
    );

    const renderMobileFooter = () => (
        <>
            <Avatar name={email || role || "User"} size={28} />
            {canAccessAdmin && (
                <Button appearance="subtle" size="small" icon={<Eye20Regular />} onClick={handleAdminView}>Вид админа</Button>
            )}
            <Button appearance="subtle" size="small" icon={<SignOut20Regular />} onClick={handleLogout}>Выйти</Button>
        </>
    );

    return (
        <BaseSidebar
            classNamePrefix="user-sidebar"
            nav={sections}
            startnav="home"
            bottomNav={bottomNav}
            renderBottom={renderBottom}
            renderMobileFooter={renderMobileFooter}
            onLogoClick={() => navigate("/home")}
            isOpen={isOpen}
            onOpenChange={(open) => dispatch(setSidebarOpen(open))}
            collapsed={useAppSelector((state) => state.ui.isSidebarCollapsed)}
            onCollapsedChange={(val) => dispatch(setSidebarCollapsed(val))}
        />
    );
};
