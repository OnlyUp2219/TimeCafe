import * as React from "react";
import {
    Hamburger,
    NavDrawer,
    NavDrawerBody,
    NavDrawerHeader,
    NavItem,
    Tooltip,
    useRestoreFocusSource,
    type NavDrawerProps,
} from "@fluentui/react-components";
import type { OnNavItemSelectData } from "@fluentui/react-components";
import "./Sidebar.css";
import { useNavigate } from "react-router-dom";

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

    const restoreFocusSourceAttributes = useRestoreFocusSource();


    const [selectedValue, setSelectedValue] = React.useState<string>("1");

    const handleItemSelect = (event: Event | React.SyntheticEvent<Element, Event>, data: OnNavItemSelectData) => {
        setSelectedValue(data.value as string);
    };
    const navigate = useNavigate();

    return (
        <aside className="app-sidebar">
            <NavDrawer
                defaultSelectedValue="1"
                onNavItemSelect={handleItemSelect}
                selectedValue={selectedValue}
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
                    <NavItem value="1" onClick={() => navigate("home")}>Главная</NavItem>
                    <NavItem value="2" onClick={() => navigate("personal-data")}>Персональные данные</NavItem>
                    <NavItem value="3" onClick={() => navigate("home")}>Тестовоя</NavItem>
                </NavDrawerBody>
            </NavDrawer>
        </aside>
    );
};
