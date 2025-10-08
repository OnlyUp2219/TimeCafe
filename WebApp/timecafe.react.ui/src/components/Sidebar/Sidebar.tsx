import * as React from "react";
import {
    Hamburger,
    NavDrawer,
    NavDrawerBody,
    NavDrawerHeader,
    NavItem,
    Tooltip,
    useRestoreFocusSource,
    useRestoreFocusTarget,
    type NavDrawerProps,
} from "@fluentui/react-components";
import type { JSXElement } from "@fluentui/react-components";
import "./Sidebar.css";

type DrawerType = Required<NavDrawerProps>["type"];

interface SidebarProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
}


export const Sidebar: React.FC<SidebarProps> = ({ isOpen, onOpenChange }) => {

    const [type, setType] = React.useState<DrawerType>("inline");

    const onMediaQueryChange = React.useCallback(
        ({ matches }: { matches: boolean }) =>
            setType(matches ? "overlay" : "inline"),
        [setType]
    );

    React.useEffect(() => {
        const match = window.matchMedia("(max-width: 720px)");

        if (match.matches) {
            setType("overlay");
        }

        match.addEventListener("change", onMediaQueryChange);

        return () => match.removeEventListener("change", onMediaQueryChange);
    }, [onMediaQueryChange]);

    // all Drawers need manual focus restoration attributes
    // unless (as in the case of some inline drawers, you do not want automatic focus restoration)
    const restoreFocusTargetAttributes = useRestoreFocusTarget();
    const restoreFocusSourceAttributes = useRestoreFocusSource();


    return (
        <aside className="app-sidebar">
            <NavDrawer
                defaultSelectedValue="1"
                defaultSelectedCategoryValue=""
                type={type}
                {...restoreFocusSourceAttributes}
                separator
                position="start"
                open={isOpen}
                onOpenChange={(_, { open }) => onOpenChange(open)}
                className="sidebar"
            >
                <NavDrawerHeader>
                    <div>
                        <Tooltip content="Close Navigation" relationship="label">
                            <Hamburger onClick={() => onOpenChange(!isOpen)} />
                        </Tooltip>
                        Основное
                    </div>
                </NavDrawerHeader>

                <NavDrawerBody>
                    <NavItem value="1">Главная</NavItem>
                    <NavItem value="2">О нас</NavItem>
                    <NavItem value="3">Контакты</NavItem>
                </NavDrawerBody>
            </NavDrawer>
        </aside>
    );
};
