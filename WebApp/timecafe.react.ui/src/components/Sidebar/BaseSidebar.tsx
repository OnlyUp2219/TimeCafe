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
    NavCategory,
    NavCategoryItem,
    NavSubItem,
    NavSubItemGroup,
    Menu,
    MenuTrigger,
    MenuPopover,
    MenuList,
    MenuItem,
    type NavDrawerProps,
} from "@fluentui/react-components";
import {
    PanelLeftContract20Regular,
    PanelLeftExpand20Regular,
} from "@fluentui/react-icons";
import { MOBILE_SIDEBAR_BREAKPOINT } from "@shared/layout/breakpoints";


import type { Permission } from "@shared/auth/permissions";

export interface NavItemType {
    id: string;
    label: string;
    path: string;
    icon: React.ReactElement;
    permission?: Permission;
    subItems?: Omit<NavItemType, "subItems">[];
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
    collapsed: boolean;
    onCollapsedChange: (collapsed: boolean) => void;
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
    collapsed,
    onCollapsedChange,
}) => {
    const navigate = useNavigate();
    const location = useLocation();
    const [drawerType, setDrawerType] = useState<DrawerType>("inline");
    const restoreFocusSourceAttributes = useRestoreFocusSource();


    const navItems = useMemo(() => {
        return nav.flatMap((section) => {
            const items: NavItemType[] = [];
            for (const item of section.items) {
                items.push(item);
                if (item.subItems) {
                    items.push(...item.subItems);
                }
            }
            return items;
        });
    }, [nav]);

    const selectedValue = useMemo(() => {
        const item = navItems.find((navItem) => location.pathname.startsWith(navItem.path));
        return item?.id ?? startnav;
    }, [location.pathname, navItems, startnav]);

    const selectedCategoryValue = useMemo(() => {
        for (const section of nav) {
            for (const item of section.items) {
                if (item.subItems?.some((sub) => location.pathname.startsWith(sub.path))) {
                    return item.id;
                }
            }
        }
        return undefined;
    }, [location.pathname, nav]);

    const [openCategories, setOpenCategories] = useState<string[]>([]);

    useEffect(() => {
        if (selectedCategoryValue && !openCategories.includes(selectedCategoryValue)) {
            setOpenCategories((prev) => [...prev, selectedCategoryValue]);
        }
    }, [selectedCategoryValue]);

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

    const handleCategoryToggle = (_: unknown, data: Parameters<NonNullable<NavDrawerProps["onNavCategoryItemToggle"]>>[1]) => {
        const categoryValue = data.categoryValue as string;
        setOpenCategories((prev) =>
            prev.includes(categoryValue)
                ? prev.filter((c) => c !== categoryValue)
                : [...prev, categoryValue]
        );
    };

    const renderNav = () => (
        <NavDrawer
            open
            type="inline"
            selectedValue={selectedValue}
            selectedCategoryValue={selectedCategoryValue}
            openCategories={openCategories}
            onNavCategoryItemToggle={handleCategoryToggle}
            onNavItemSelect={handleDrawerItemSelect}
            className={`${classNamePrefix}__nav`}
            style={{ backgroundColor: "var(--colorNeutralBackground1)" }}
        >
            <NavDrawerBody className="!pt-0">
                {nav.map((section) => (
                    <div key={section.title}>
                        <NavSectionHeader>{section.title}</NavSectionHeader>
                        {section.items.map((item) => {
                            if (item.subItems && item.subItems.length > 0) {
                                return (
                                    <NavCategory key={item.id} value={item.id}>
                                        <NavCategoryItem icon={item.icon} className="admin-sidebar__navItem">
                                            {item.label}
                                        </NavCategoryItem>
                                        <NavSubItemGroup>
                                            {item.subItems.map((subItem) => (
                                                <NavSubItem key={subItem.id} value={subItem.id} className="admin-sidebar__navItem">
                                                    {subItem.label}
                                                </NavSubItem>
                                            ))}
                                        </NavSubItemGroup>
                                    </NavCategory>
                                );
                            }
                            return (
                                <NavItem key={item.id} value={item.id} icon={item.icon} className="admin-sidebar__navItem">
                                    {item.label}
                                </NavItem>
                            );
                        })}
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
                    selectedCategoryValue={selectedCategoryValue}
                    openCategories={openCategories}
                    onNavCategoryItemToggle={handleCategoryToggle}
                    onNavItemSelect={handleDrawerItemSelect}
                    className={`${classNamePrefix}__nav admin-sidebar__nav--mobile`}
                    style={{ backgroundColor: "var(--colorNeutralBackground1)" }}
                >
                    <NavDrawerBody className="!pt-0">
                        {nav.map((section) => (
                            <div key={section.title}>
                                <NavSectionHeader>{section.title}</NavSectionHeader>
                                {section.items.map((item) => {
                                    if (item.subItems && item.subItems.length > 0) {
                                        return (
                                            <NavCategory key={item.id} value={item.id}>
                                                <NavCategoryItem icon={item.icon} className="admin-sidebar__navItem">
                                                    {item.label}
                                                </NavCategoryItem>
                                                <NavSubItemGroup>
                                                    {item.subItems.map((subItem) => (
                                                        <NavSubItem key={subItem.id} value={subItem.id} className="admin-sidebar__navItem">
                                                            {subItem.label}
                                                        </NavSubItem>
                                                    ))}
                                                </NavSubItemGroup>
                                            </NavCategory>
                                        );
                                    }
                                    return (
                                        <NavItem key={item.id} value={item.id} icon={item.icon} className="admin-sidebar__navItem">
                                            {item.label}
                                        </NavItem>
                                    );
                                })}
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
                <div className="sticky top-0 z-10 max-h-[calc(100dvh-100px)] overflow-y-auto bg-(--colorNeutralBackground1) flex flex-col hide-scrollbar">
                    <div className={collapsed ? `${classNamePrefix}__header admin-sidebar__header--collapsed` : `${classNamePrefix}__header`}>
                        <Tooltip content={collapsed ? "Развернуть" : "Свернуть"} relationship="label">
                            <Button
                                appearance="subtle"
                                size="large"
                                icon={collapsed ? <PanelLeftExpand20Regular /> : <PanelLeftContract20Regular />}
                                onClick={() => onCollapsedChange(!collapsed)}
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
                                {nav.flatMap((section) => section.items).map((item) => {
                                    const hasSubItems = item.subItems && item.subItems.length > 0;
                                    const isSubActive = hasSubItems && (item.subItems!.some((sub) => selectedValue === sub.id) || selectedCategoryValue === item.id);
                                    const isActive = selectedValue === item.id || isSubActive;

                                    if (hasSubItems) {
                                        return (
                                            <Menu key={item.id} positioning="after-top">
                                                <MenuTrigger disableButtonEnhancement>
                                                    <Tooltip content={item.label} relationship="label" positioning="after">
                                                        <Button
                                                            appearance={isActive ? "primary" : "subtle"}
                                                            size="large"
                                                            icon={item.icon}
                                                            className="admin-sidebar__icon-btn"
                                                        />
                                                    </Tooltip>
                                                </MenuTrigger>
                                                <MenuPopover>
                                                    <MenuList>
                                                        {item.subItems!.map((subItem) => (
                                                            <MenuItem
                                                                key={subItem.id}
                                                                icon={subItem.icon}
                                                                onClick={() => handleNavigate(subItem.path)}
                                                                className={`admin-sidebar__navItem ${selectedValue === subItem.id ? "admin-sidebar__navItem--selected" : ""}`}
                                                            >
                                                                {subItem.label}
                                                            </MenuItem>
                                                        ))}
                                                    </MenuList>
                                                </MenuPopover>
                                            </Menu>
                                        );
                                    }

                                    return (
                                        <Tooltip key={item.id} content={item.label} relationship="label" positioning="after">
                                            <Button
                                                appearance={selectedValue === item.id ? "primary" : "subtle"}
                                                size="large"
                                                icon={item.icon}
                                                onClick={() => handleNavigate(item.path)}
                                                className="admin-sidebar__icon-btn"
                                            />
                                        </Tooltip>
                                    );
                                })}
                            </nav>
                        ) : (
                            renderNav()
                        )}
                    </div>
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
