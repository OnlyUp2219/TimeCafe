import { type FC, useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
    Button,
    DrawerBody,
    DrawerFooter,
    DrawerHeader,
    DrawerHeaderTitle,
    NavDrawer,
    NavDrawerBody,
    NavItem,
    NavSectionHeader,
    OverlayDrawer,
    Tooltip,
    useRestoreFocusSource,
    type NavDrawerProps,
} from "@fluentui/react-components";
import {
    PanelLeftContract20Regular,
    PanelLeftExpand20Regular,
} from "@fluentui/react-icons";
import { MOBILE_SIDEBAR_BREAKPOINT } from "@shared/layout/breakpoints";

export interface NavItemType {
    id: string;
    label: string;
    path: string;
    icon: React.ReactElement;
}

export interface NavSectionType {
    title: string;
    items: NavItemType[];
}

export interface BaseSidebarProps {
    classNamePrefix: "user-sidebar" | "admin-sidebar";
    nav: NavSectionType[];
    startnav: string;
    bottomNav?: NavItemType[];
    renderBottom: (compact: boolean) => React.ReactNode;
    renderMobileFooter: () => React.ReactNode;
    logoText?: string;
    onLogoClick?: () => void;
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
    showMobileToggle?: boolean;
}

type DrawerType = Required<NavDrawerProps>["type"];

export const BaseSidebar: FC<BaseSidebarProps> = ({
    classNamePrefix,
    nav,
    startnav,
    bottomNav = [],
    renderBottom,
    renderMobileFooter,
    logoText = "☕ TimeCafe",
    onLogoClick,
    isOpen,
    onOpenChange,
    showMobileToggle = false,
}) => {
    const navigate = useNavigate();
    const location = useLocation();
    const [collapsed, setCollapsed] = useState(false);
    const [drawerType, setDrawerType] = useState<DrawerType>("inline");
    const restoreFocusSourceAttributes = useRestoreFocusSource();

    const navItems = useMemo(() => nav.flatMap((section) => section.items), [nav]);

    const selectedValue = useMemo(() => {
        const item = navItems.find((navItem) => location.pathname.startsWith(navItem.path));
        return item?.id ?? startnav;
    }, [location.pathname, navItems, startnav]);

    useEffect(() => {
        const mediaQuery = globalThis.matchMedia(`(max-width: ${MOBILE_SIDEBAR_BREAKPOINT}px)`);
        const updateDrawerType = (matches: boolean) => {
            setDrawerType(matches ? "overlay" : "inline");
            if (!matches && isOpen) {
                onOpenChange(false);
            }
        };

        updateDrawerType(mediaQuery.matches);
        const listener = ({ matches }: MediaQueryListEvent) => updateDrawerType(matches);
        mediaQuery.addEventListener("change", listener);
        return () => mediaQuery.removeEventListener("change", listener);
    }, [isOpen, onOpenChange]);

    const handleNavigate = (path: string) => {
        navigate(path);
        onOpenChange(false);
    };

    const handleDrawerItemSelect = (_: unknown, data: Parameters<NonNullable<NavDrawerProps["onNavItemSelect"]>>[1]) => {
        const item = navItems.find((navItem) => navItem.id === String(data.value));
        if (item) {
            handleNavigate(item.path);
        }
    };

    const renderNav = () => (
        <NavDrawer
            open
            type="inline"
            selectedValue={selectedValue}
            onNavItemSelect={handleDrawerItemSelect}
            className={`${classNamePrefix}__nav`}
            style={{ backgroundColor: "var(--colorNeutralBackground1)" }}
        >
            <NavDrawerBody className="!pt-0">
                {nav.map((section) => (
                    <div key={section.title}>
                        <NavSectionHeader>{section.title}</NavSectionHeader>
                        {section.items.map((item) => (
                            <NavItem key={item.id} value={item.id} icon={item.icon} className="admin-sidebar__navItem">
                                {item.label}
                            </NavItem>
                        ))}
                    </div>
                ))}
            </NavDrawerBody>
        </NavDrawer>
    );

    const renderMobileDrawer = () => (
        <OverlayDrawer
            {...restoreFocusSourceAttributes}
            open={isOpen}
            position="start"
            size="full"
            onOpenChange={(_, data) => onOpenChange(data.open)}
            className={`${classNamePrefix}__mobile-drawer`}
        >
            <DrawerHeader>
                <DrawerHeaderTitle
                    action={
                        <Button
                            appearance="subtle"
                            size="large"
                            icon={<PanelLeftContract20Regular />}
                            aria-label="Скрыть меню"
                            onClick={() => onOpenChange(false)}
                        />
                    }
                >
                    Меню
                </DrawerHeaderTitle>
            </DrawerHeader>

            <DrawerBody className={`${classNamePrefix}__mobile-drawer-body`}>
                <NavDrawer
                    open
                    type="inline"
                    selectedValue={selectedValue}
                    onNavItemSelect={handleDrawerItemSelect}
                    className={`${classNamePrefix}__nav admin-sidebar__nav--mobile`}
                    style={{ backgroundColor: "var(--colorNeutralBackground1)" }}
                >
                    <NavDrawerBody className="!pt-0">
                        {nav.map((section) => (
                            <div key={section.title}>
                                <NavSectionHeader>{section.title}</NavSectionHeader>
                                {section.items.map((item) => (
                                    <NavItem key={item.id} value={item.id} icon={item.icon} className="admin-sidebar__navItem">
                                        {item.label}
                                    </NavItem>
                                ))}
                            </div>
                        ))}
                    </NavDrawerBody>
                </NavDrawer>
            </DrawerBody>

            <DrawerFooter className={`${classNamePrefix}__mobile-footer`}>
                {renderMobileFooter()}
            </DrawerFooter>
        </OverlayDrawer>
    );

    return (
        <>
            {showMobileToggle && (
                <Button
                    {...restoreFocusSourceAttributes}
                    aria-label={isOpen ? "Закрыть меню" : "Открыть меню"}
                    appearance="subtle"
                    size="large"
                    icon={<PanelLeftExpand20Regular />}
                    onClick={() => onOpenChange(!isOpen)}
                    className={`${classNamePrefix}__mobile-toggle`}
                />
            )}

            <aside className={classNamePrefix} data-collapsed={collapsed} data-mobile-open={isOpen}>
                <div className={collapsed ? `${classNamePrefix}__header admin-sidebar__header--collapsed` : `${classNamePrefix}__header`}>
                    <Tooltip content={collapsed ? "Развернуть" : "Свернуть"} relationship="label">
                        <Button
                            appearance="subtle"
                            size="large"
                            icon={collapsed ? <PanelLeftExpand20Regular /> : <PanelLeftContract20Regular />}
                            onClick={() => setCollapsed((prev) => !prev)}
                            className={collapsed ? "mx-auto" : "ml-auto"}
                        />
                    </Tooltip>
                    {!collapsed && (
                        <div className={`${classNamePrefix}__logo`} onClick={onLogoClick}>
                            {logoText}
                        </div>
                    )}
                </div>

                <div className={`${classNamePrefix}__desktop-nav`}>
                    {collapsed ? (
                        <nav className="admin-sidebar__collapsed-nav pt-0">
                            {navItems.map((item) => (
                                <Tooltip key={item.id} content={item.label} relationship="label" positioning="after">
                                    <Button
                                        appearance={selectedValue === item.id ? "primary" : "subtle"}
                                        size="large"
                                        icon={item.icon}
                                        onClick={() => handleNavigate(item.path)}
                                        className="admin-sidebar__icon-btn"
                                    />
                                </Tooltip>
                            ))}
                        </nav>
                    ) : (
                        renderNav()
                    )}
                </div>

                <div className={`${classNamePrefix}__bottom`}>
                    {renderBottom(collapsed)}
                </div>
            </aside>

            {drawerType === "overlay" && isOpen && renderMobileDrawer()}

            {drawerType === "inline" && bottomNav.length > 0 && (
                <div className={`${classNamePrefix}__mobile-bottom-nav`}>
                    {bottomNav.map((item) => (
                        <Button
                            key={item.id}
                            appearance={selectedValue === item.id ? "primary" : "subtle"}
                            size="large"
                            icon={item.icon}
                            aria-label={item.label}
                            onClick={() => handleNavigate(item.path)}
                            className={`${classNamePrefix}__mobile-bottom-nav-item`}
                        />
                    ))}
                    <Button
                        appearance="subtle"
                        size="large"
                        icon={<PanelLeftExpand20Regular />}
                        aria-label="Открыть меню"
                        onClick={() => onOpenChange(true)}
                        className={`${classNamePrefix}__mobile-bottom-nav-item`}
                    />
                </div>
            )}
        </>
    );
};
