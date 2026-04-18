import { type FC, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Avatar, Body1, Button, Caption1, Tooltip } from "@fluentui/react-components";
import { useAppDispatch, useAppSelector } from "@store/hooks";
import { clearTokens } from "@store/authSlice";
import {
    Board20Regular, People20Regular, Clock20Regular, Money20Regular,
    Gift20Regular, Color20Regular, ArrowTrending20Regular, Payment20Regular,
    SignOut20Regular, Eye20Regular
} from "@fluentui/react-icons";
import { BaseSidebar, type NavSectionType, type NavItemType } from "@components/Sidebar/BaseSidebar";

interface AdminSidebarProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
}

export const AdminSidebar: FC<AdminSidebarProps> = ({ isOpen, onOpenChange }) => {
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const email = useAppSelector((state) => state.auth.email);

    const sections: NavSectionType[] = useMemo(() => [
        {
            title: "Основное",
            items: [
                { id: "dashboard", label: "Дашборд", path: "/admin/dashboard", icon: <Board20Regular /> },
                { id: "users", label: "Пользователи", path: "/admin/users", icon: <People20Regular /> },
                { id: "roles", label: "Роли", path: "/admin/roles", icon: <People20Regular /> },
                { id: "visits", label: "Визиты", path: "/admin/visits", icon: <Clock20Regular /> },
            ],
        },
        {
            title: "Управление",
            items: [
                { id: "tariffs", label: "Тарифы", path: "/admin/tariffs", icon: <Money20Regular /> },
                { id: "promotions", label: "Акции", path: "/admin/promotions", icon: <Gift20Regular /> },
                { id: "themes", label: "Темы оформления", path: "/admin/themes", icon: <Color20Regular /> },
            ],
        },
        {
            title: "Финансы",
            items: [
                { id: "balances", label: "Балансы", path: "/admin/balances", icon: <Money20Regular /> },
                { id: "transactions", label: "Транзакции", path: "/admin/transactions", icon: <ArrowTrending20Regular /> },
                { id: "payments", label: "Платежи", path: "/admin/payments", icon: <Payment20Regular /> },
            ],
        },
    ], []);

    const bottomNav: NavItemType[] = useMemo(() => sections.flatMap((section) => section.items).slice(0, 4), [sections]);

    const handleLogout = () => {
        dispatch(clearTokens());
        navigate("/login", { replace: true });
    };

    const renderBottom = (compact: boolean) => (
        <>
            {compact ? (
                <div className="admin-sidebar__bottom-compact">
                    <Avatar name={email ?? "Admin"} size={28} />
                    <Tooltip content="Вид клиента" relationship="label" positioning="after">
                        <Button appearance="subtle" size="small" icon={<Eye20Regular />} onClick={() => navigate("/home")} />
                    </Tooltip>
                    <Tooltip content="Выйти" relationship="label" positioning="after">
                        <Button appearance="subtle" size="small" icon={<SignOut20Regular />} onClick={handleLogout} />
                    </Tooltip>
                </div>
            ) : (
                <>
                    <div className="admin-sidebar__bottom-user">
                        <Avatar name={email ?? "Admin"} size={32} />
                        <div className="min-w-0">
                            <Body1 truncate wrap={false} block>Админ</Body1>
                            <Caption1 truncate wrap={false} block>{email ?? "—"}</Caption1>
                        </div>
                    </div>
                    <div className="admin-sidebar__bottom-actions">
                        <Button appearance="subtle" size="small" icon={<Eye20Regular />} onClick={() => navigate("/home")}>Вид клиента</Button>
                        <Button appearance="subtle" size="small" icon={<SignOut20Regular />} onClick={handleLogout}>Выйти</Button>
                    </div>
                </>
            )}
        </>
    );

    const renderMobileFooter = () => (
        <>
            <Avatar name={email ?? "Admin"} size={28} />
            <Button appearance="subtle" size="small" icon={<Eye20Regular />} onClick={() => { onOpenChange(false); navigate("/home"); }}>Вид клиента</Button>
            <Button appearance="subtle" size="small" icon={<SignOut20Regular />} onClick={handleLogout}>Выйти</Button>
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
        />
    );
};
