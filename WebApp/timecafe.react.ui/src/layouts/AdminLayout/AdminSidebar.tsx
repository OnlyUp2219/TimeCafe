import { type FC, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { Body1, Button, Caption1, Tooltip, CounterBadge } from "@fluentui/react-components";
import { useAppDispatch, useAppSelector } from "@store/hooks";
import { clearTokens } from "@store/authSlice";
import { setSidebarCollapsed, toggleTheme } from "@store/uiSlice";
import {
    Board20Regular, People20Regular, Clock20Regular, Money20Regular,
    Gift20Regular, Color20Regular, ArrowTrending20Regular, Payment20Regular,
    SignOut20Regular, Eye20Regular, Bug24Regular, ShieldSettings20Regular,
    DocumentText20Regular, Grid20Regular, WeatherMoon20Regular, WeatherSunny20Regular
} from "@fluentui/react-icons";
import { BaseSidebar, type NavSectionType, type NavItemType } from "@components/Sidebar/BaseSidebar";
import { Permissions } from "@shared/auth/permissions";
import { NO_DATA } from "@shared/const/placeholders";
import { usePermissions } from "@hooks/usePermissions";
import { SecureAvatar } from "@components/SecureAvatar/SecureAvatar";
import { useGetProfileByUserIdQuery } from "@store/api/profileApi";
import { useGetUsersCompositeQuery } from "@store/api/adminApi";
import { useGetPendingVisitsQuery } from "@store/api/venueApi";

interface AdminSidebarProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
}

export const AdminSidebar: FC<AdminSidebarProps> = ({ isOpen, onOpenChange }) => {
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const email = useAppSelector((state) => state.auth.email);
    const displayName = useAppSelector((state) => state.auth.displayName);
    const userId = useAppSelector((state) => state.auth.userId);
    const theme = useAppSelector((state) => state.ui.theme);
    const { data: profile } = useGetProfileByUserIdQuery(userId ?? "", { skip: !userId });
    const profileDisplayName = profile ? `${profile.firstName?.trim() ?? ""} ${profile.lastName?.trim() ?? ""}`.trim() : null;
    const avatarName = profileDisplayName || displayName || email?.trim() || "Admin";

    const { has } = usePermissions();

    const hasAccountAdminRead = has(Permissions.AccountAdminRead);
    const hasVenueVisitRead = has(Permissions.VenueVisitRead);

    const { data: usersData } = useGetUsersCompositeQuery(
        { page: 1, size: 1 },
        { skip: !hasAccountAdminRead, pollingInterval: 5000 }
    );
    const { data: pendingVisitsData } = useGetPendingVisitsQuery(
        { page: 1, pageSize: 1 },
        { skip: !hasVenueVisitRead, pollingInterval: 5000 }
    );

    const totalUsersCount = usersData?.metadata?.totalCount ?? 0;
    const pendingVisitsCount = pendingVisitsData?.metadata?.totalCount ?? 0;

    const sections: NavSectionType[] = useMemo(() => {
        const allSections: NavSectionType[] = [
            {
                title: "Основное",
                items: [
                    {
                        id: "dashboard",
                        label: "Дашборд",
                        path: "/admin/dashboard",
                        icon: <Board20Regular />,
                        permission: Permissions.AccountAdminRead,
                        subItems: [
                            { id: "dashboard-main", label: "Обзор", path: "/admin/dashboard", icon: <Board20Regular />, permission: Permissions.AccountAdminRead },
                            { id: "grafana", label: "Grafana", path: "/admin/monitoring/grafana", icon: <Board20Regular />, permission: Permissions.AccountAdminRead },
                            { id: "kibana", label: "Kibana", path: "/admin/monitoring/kibana", icon: <Board20Regular />, permission: Permissions.AccountAdminRead }
                        ]
                    },
                    {
                        id: "users",
                        label: "Пользователи",
                        path: "/admin/users",
                        icon: <People20Regular />,
                        permission: Permissions.AccountAdminRead,
                        badge: totalUsersCount > 0 ? (
                            <CounterBadge count={totalUsersCount} color="informative" size="small" />
                        ) : undefined
                    },
                    { id: "roles", label: "Роли", path: "/admin/roles", icon: <ShieldSettings20Regular />, permission: Permissions.RbacRoleRead },
                    {
                        id: "visits",
                        label: "Визиты",
                        path: "/admin/visits",
                        icon: <Clock20Regular />,
                        permission: Permissions.VenueVisitRead,
                        badge: pendingVisitsCount > 0 ? (
                            <CounterBadge count={pendingVisitsCount} color="danger" size="small" />
                        ) : undefined
                    },
                    { id: "resources", label: "Карта столов", path: "/admin/resources", icon: <Grid20Regular />, permission: Permissions.VenueVisitRead },
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
                    { id: "audit-logs", label: "Аудит-логи", path: "/admin/audit-logs", icon: <DocumentText20Regular />, permission: Permissions.AuditLogAdminRead },
                    { id: "debug", label: "Debug Errors", path: "/admin/dev-debug", icon: <Bug24Regular /> },
                ],
            },
        ];

        return allSections
            .map(section => ({
                ...section,
                items: section.items
                    .filter(item => !item.permission || has(item.permission))
                    .map(item => {
                        if (item.subItems) {
                            return {
                                ...item,
                                subItems: item.subItems.filter(sub => !sub.permission || has(sub.permission))
                            };
                        }
                        return item;
                    })
            }))
            .filter(section => section.items.length > 0);
    }, [has, totalUsersCount, pendingVisitsCount]);

    const bottomNav: NavItemType[] = useMemo(() => sections.flatMap((section) => section.items).slice(0, 4), [sections]);

    const handleLogout = () => {
        dispatch(clearTokens());
        navigate("/login", { replace: true });
    };

    const renderBottom = (compact: boolean) => (
        <>
            {compact ? (
                <div className="admin-sidebar__bottom-compact">
                    <SecureAvatar name={avatarName} photoUrl={profile?.photoUrl} size={28} />
                    <Tooltip content="Сменить тему" relationship="label" positioning="after">
                        <Button appearance="subtle" size="small" icon={theme === "dark" ? <WeatherSunny20Regular /> : <WeatherMoon20Regular />} onClick={() => dispatch(toggleTheme())} />
                    </Tooltip>
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
                        <SecureAvatar name={avatarName} photoUrl={profile?.photoUrl} size={32} />
                        <div className="min-w-0 flex flex-col">
                            <Body1 truncate wrap={false} title={avatarName}>{avatarName || "Админ"}</Body1>
                            <Caption1 truncate wrap={false} title={email ?? undefined}>
                                {email && email.length > 20 ? `${email.substring(0, 20)}...` : email || NO_DATA}
                            </Caption1>
                        </div>
                    </div>
                    <div className="admin-sidebar__bottom-actions">
                        <Tooltip content="Сменить тему" relationship="label" positioning="above">
                            <Button appearance="subtle" size="small" icon={theme === "dark" ? <WeatherSunny20Regular /> : <WeatherMoon20Regular />} onClick={() => dispatch(toggleTheme())} />
                        </Tooltip>
                        <Button appearance="subtle" size="small" icon={<Eye20Regular />} onClick={() => navigate("/home")}>Вид клиента</Button>
                        <Button appearance="subtle" size="small" icon={<SignOut20Regular />} onClick={handleLogout}>Выйти</Button>
                    </div>
                </>
            )}
        </>
    );

    const renderMobileFooter = () => (
        <>
            <SecureAvatar name={avatarName} photoUrl={profile?.photoUrl} size={28} />
            <Button appearance="subtle" size="small" icon={theme === "dark" ? <WeatherSunny20Regular /> : <WeatherMoon20Regular />} onClick={() => dispatch(toggleTheme())} aria-label="Сменить тему" />
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
            collapsed={useAppSelector((state) => state.ui.isSidebarCollapsed)}
            onCollapsedChange={(val) => dispatch(setSidebarCollapsed(val))}
        />
    );
};
