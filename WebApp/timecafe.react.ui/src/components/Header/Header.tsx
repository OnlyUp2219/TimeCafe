import * as React from "react";
import {Hamburger, Button, Avatar} from "@fluentui/react-components";
import "./Header.css";
import {LogOut} from "../../api/auth.ts";
import {useDispatch} from "react-redux";

interface HeaderProps {
    onMenuToggle?: () => void;
    isSidebarOpen?: boolean;
}


export const Header: React.FC<HeaderProps> = ({onMenuToggle, isSidebarOpen}) => {
    const dispatch = useDispatch();
    return (
        <header className="app-header">
            <div className="app-header__left">
                {!isSidebarOpen && onMenuToggle && (
                    <Hamburger
                        aria-label="Toggle sidebar"
                        onClick={onMenuToggle}
                        className="app-header__hamburger"
                    />
                )}

                <h1 className="app-header__title">TimeCafe</h1>
            </div>

            <div className="app-header__right gap-[12px]">
                <Avatar/>
                <Button appearance="primary" onClick={() => LogOut(dispatch)}>Выйти</Button>
            </div>
        </header>
    );
};
