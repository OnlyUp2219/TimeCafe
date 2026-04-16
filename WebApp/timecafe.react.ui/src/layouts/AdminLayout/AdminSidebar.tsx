import {type FC, useMemo, useState} from "react";
import {useLocation, useNavigate} from "react-router-dom";
import {useAppDispatch, useAppSelector} from "@store/hooks";
import {selectUserId} from "@store/authSlice";
import {clearTokens} from "@store/authSlice";
import {Avatar, Button, Body1, Caption1, Tooltip} from "@fluentui/react-components";
import {
    NavDrawer,
    NavDrawerBody,
    NavItem,
    NavSectionHeader,
} from "@fluentui/react-components";
import {
    Board20Regular,
    People20Regular,
    Clock20Regular,
    Money20Regular,
    Gift20Regular,
    Color20Regular,
    ArrowTrending20Regular,
    Payment20Regular,
    SignOut20Regular,
    Eye20Regular,
    PanelLeftContract20Regular,
    PanelLeftExpand20Regular,
} from "@fluentui/react-icons";

interface AdminNavItem {
    id: string;
    label: string;
    path: string;
    icon: React.ReactElement;
}

interface AdminNavSection {
    title: string;
    items: AdminNavItem[];
}

export const AdminSidebar: FC = () => {
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const location = useLocation();
    const email = useAppSelector((state) => state.auth.email);
    const userId = useAppSelector(selectUserId);
    const [collapsed, setCollapsed] = useState(false);

    const sections: AdminNavSection[] = useMemo(() => [
        {
            title: "Основное",
            items: [
                {id: "dashboard", label: "Дашборд", path: "/admin/dashboard", icon: <Board20Regular />},
                {id: "users", label: "Пользователи", path: "/admin/users", icon: <People20Regular />},
                {id: "visits", label: "Визиты", path: "/admin/visits", icon: <Clock20Regular />},
            ],
        },
        {
            title: "Управление",
            items: [
                {id: "tariffs", label: "Тарифы", path: "/admin/tariffs", icon: <Money20Regular />},
                {id: "promotions", label: "Акции", path: "/admin/promotions", icon: <Gift20Regular />},
                {id: "themes", label: "Темы оформления", path: "/admin/themes", icon: <Color20Regular />},
            ],
        },
        {
            title: "Финансы",
            items: [
                {id: "balances", label: "Балансы", path: "/admin/balances", icon: <Money20Regular />},
                {id: "transactions", label: "Транзакции", path: "/admin/transactions", icon: <ArrowTrending20Regular />},
                {id: "payments", label: "Платежи", path: "/admin/payments", icon: <Payment20Regular />},
            ],
        },
    ], []);

    const selectedValue = useMemo(() => {
        for (const section of sections) {
            const item = section.items.find(i => location.pathname.startsWith(i.path));
            if (item) return item.id;
        }
        return "dashboard";
    }, [location.pathname, sections]);

    const handleLogout = () => {
        dispatch(clearTokens());
        navigate("/login", {replace: true});
    };

    const sidebarWidth = collapsed ? 56 : 260;

    return (
        <aside className="admin-sidebar" style={{width: sidebarWidth}} data-collapsed={collapsed}>
            <div className="admin-sidebar__header">
                <Tooltip content={collapsed ? "Развернуть" : "Свернуть"} relationship="label">
                    <Button
                        appearance="subtle"
                        size="large"
                        icon={collapsed ? <PanelLeftExpand20Regular /> : <PanelLeftContract20Regular />}
                        onClick={() => setCollapsed(prev => !prev)}
                        className={collapsed ? "mx-auto" : "ml-auto"}
                    />
                </Tooltip>
                                {!collapsed && (
                    <div className="admin-sidebar__logo" onClick={() => navigate("/admin/dashboard")}>
                        ☕ TimeCafe
                    </div>
                )}
            </div>

            {collapsed ? (
                <nav className="admin-sidebar__collapsed-nav">
                    {sections.flatMap(section =>
                        section.items.map(item => (
                            <Tooltip key={item.id} content={item.label} relationship="label" positioning="after">
                                <Button
                                    appearance={selectedValue === item.id ? "primary" : "subtle"}
                                    size="medium"
                                    icon={item.icon}
                                    onClick={() => navigate(item.path)}
                                    className="admin-sidebar__icon-btn"
                                />
                            </Tooltip>
                        ))
                    )}
                </nav>
            ) : (
                <NavDrawer
                    open
                    type="inline"
                    selectedValue={selectedValue}
                    onNavItemSelect={(_, data) => {
                        for (const section of sections) {
                            const item = section.items.find(i => i.id === String(data.value));
                            if (item) {
                                navigate(item.path);
                                return;
                            }
                        }
                    }}
                    className="admin-sidebar__nav"
                    style={{backgroundColor: "var(--colorNeutralBackground1)"}}
                >
                    <NavDrawerBody>
                        {sections.map((section) => (
                            <div key={section.title}>
                                <NavSectionHeader>{section.title}</NavSectionHeader>
                                {section.items.map((item) => (
                                    <NavItem
                                        key={item.id}
                                        value={item.id}
                                        icon={item.icon}
                                        className="admin-sidebar__navItem"
                                    >
                                        {item.label}
                                    </NavItem>
                                ))}
                            </div>
                        ))}
                    </NavDrawerBody>
                </NavDrawer>
            )}

            <div className="admin-sidebar__bottom">
                {collapsed ? (
                    <div className="flex flex-col items-center gap-2">
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
                        <div className="flex items-center gap-2">
                            <Avatar name={email ?? "Admin"} size={32} />
                            <div className="min-w-0">
                                <Body1 truncate wrap={false} block>Админ</Body1>
                                <Caption1 truncate wrap={false} block>{email ?? "—"}</Caption1>
                            </div>
                        </div>
                        <div className="flex gap-2 mt-2">
                            <Button
                                appearance="subtle"
                                size="small"
                                icon={<Eye20Regular />}
                                onClick={() => navigate("/home")}
                            >
                                Вид клиента
                            </Button>
                            <Button
                                appearance="subtle"
                                size="small"
                                icon={<SignOut20Regular />}
                                onClick={handleLogout}
                            >
                                Выйти
                            </Button>
                        </div>
                    </>
                )}
            </div>
        </aside>
    );
};
