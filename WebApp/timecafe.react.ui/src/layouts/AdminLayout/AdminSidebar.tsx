import { type FC, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Avatar, Body1, Button, Caption1, Tooltip } from "@fluentui/react-components";
import { useAppDispatch, useAppSelector } from "@store/hooks";
import { clearTokens } from "@store/authSlice";
import { setSidebarCollapsed } from "@store/uiSlice";
import {
    Board20Regular, People20Regular, Clock20Regular, Money20Regular,
    Gift20Regular, Color20Regular, ArrowTrending20Regular, Payment20Regular,
    SignOut20Regular, Eye20Regular, Bug24Regular
} from "@fluentui/react-icons";
import { BaseSidebar, type NavSectionType, type NavItemType } from "@components/Sidebar/BaseSidebar";
import { Permissions } from "@shared/auth/permissions";
import { usePermissions } from "@hooks/usePermissions";
import { useComponentSize } from "@hooks/useComponentSize";

interface AdminSidebarProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
}

export const AdminSidebar: FC<AdminSidebarProps> = ({ isOpen, onOpenChange }) => {
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const email = useAppSelector((state) => state.auth.email);

    const { has, loaded } = usePermissions();
    const { sizes } = useComponentSize();

    const sections: NavSectionType[] = useMemo(() => {
        const allSections: NavSectionType[] = [
            {
                title: "Основное",
                items: [
                    { id: "dashboard", label: "Дашборд", path: "/admin/dashboard", icon: <Board20Regular />, permission: Permissions.AccountAdminRead },
                    { id: "users", label: "Пользователи", path: "/admin/users", icon: <People20Regular />, permission: Permissions.AccountAdminRead },
                    { id: "roles", label: "Роли", path: "/admin/roles", icon: <People20Regular />, permission: Permissions.RbacRoleRead },
                    { id: "visits", label: "Визиты", path: "/admin/visits", icon: <Clock20Regular />, permission: Permissions.VenueVisitRead },
                ],
            },
            {
                title: "Управление",
                items: [
                    { id: "tariffs", label: "Тарифы", path: "/admin/tariffs", icon: <Money20Regular />, permission: Permissions.VenueTariffRead },
                    { id: "promotions", label: "Акции", path: "/admin/promotions", icon: <Gift20Regular />, permission: Permissions.VenuePromotionRead },
                    { id: "themes", label: "Темы оформления", path: "/admin/themes", icon: <Color20Regular />, permission: Permissions.VenueThemeRead },
                ],
            },
            {
                title: "Финансы",
                items: [
                    { id: "balances", label: "Балансы", path: "/admin/balances", icon: <Money20Regular />, permission: Permissions.BillingBalanceRead },
                    { id: "transactions", label: "Транзакции", path: "/admin/transactions", icon: <ArrowTrending20Regular />, permission: Permissions.BillingTransactionRead },
                    { id: "payments", label: "Платежи", path: "/admin/payments", icon: <Payment20Regular />, permission: Permissions.BillingPaymentHistoryRead },
                ],
            },
            {
                title: "Система",
                items: [
                    { id: "debug", label: "Debug Errors", path: "/admin/dev-debug", icon: <Bug24Regular /> },
                ],
            },
        ];

        return allSections
            .map(section => ({
                ...section,
                items: section.items.filter(item => !item.permission || has(item.permission))
            }))
            .filter(section => section.items.length > 0);
    }, [has]);

    const bottomNav: NavItemType[] = useMemo(() => sections.flatMap((section) => section.items).slice(0, 4), [sections]);

    const handleLogout = () => {
        dispatch(clearTokens());
        navigate("/login", { replace: true });
    };

    const renderBottom = (compact: boolean) => (
        <>
            {compact ? (
                <div className="admin-sidebar__bottom-compact">
                    <Avatar name={email ?? "Admin"} size={sizes.avatar} />
                    <Tooltip content="Вид клиента" relationship="label" positioning="after">
                        <Button appearance="subtle" size={sizes.button} icon={<Eye20Regular />} onClick={() => navigate("/home")} />
                    </Tooltip>
                    <Tooltip content="Выйти" relationship="label" positioning="after">
                        <Button appearance="subtle" size={sizes.button} icon={<SignOut20Regular />} onClick={handleLogout} />
                    </Tooltip>
                </div>
            ) : (
                <>
                    <div className="admin-sidebar__bottom-user">
                        <Avatar name={email ?? "Admin"} size={sizes.avatar} />
                        <div className="min-w-0">
                            <Body1 truncate wrap={false} block>Админ</Body1>
                            <Caption1 truncate wrap={false} block>{email ?? "—"}</Caption1>
                        </div>
                    </div>
                    <div className="admin-sidebar__bottom-actions">
                        <Button appearance="subtle" size={sizes.button} icon={<Eye20Regular />} onClick={() => navigate("/home")}>Вид клиента</Button>
                        <Button appearance="subtle" size={sizes.button} icon={<SignOut20Regular />} onClick={handleLogout}>Выйти</Button>
                    </div>
                </>
            )}
        </>
    );

    const renderMobileFooter = () => (
        <>
            <Avatar name={email ?? "Admin"} size={sizes.avatar} />
            <Button appearance="subtle" size={sizes.button} icon={<Eye20Regular />} onClick={() => { onOpenChange(false); navigate("/home"); }}>Вид клиента</Button>
            <Button appearance="subtle" size={sizes.button} icon={<SignOut20Regular />} onClick={handleLogout}>Выйти</Button>
        </>
    );

    return (
        <BaseSidebar
            classNamePrefix="admin-sidebar"
            nav={sections}
            startnav="dashboard"
            bottomNav={bottomNav}
            renderBottom={renderBottom}
            renderMobileFooter={renderMobileFooter}
            onLogoClick={() => navigate("/admin/dashboard")}
            isOpen={isOpen}
            onOpenChange={onOpenChange}
            collapsed={useAppSelector((state) => state.ui.isSidebarCollapsed)}
            onCollapsedChange={(val) => dispatch(setSidebarCollapsed(val))}
        />
    );
};
