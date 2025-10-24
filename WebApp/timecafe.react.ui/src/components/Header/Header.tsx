import {Hamburger, Button, Avatar} from "@fluentui/react-components";
import "./Header.css";
import {logoutServer} from "../../api/auth.ts";
import {useDispatch, useSelector} from "react-redux";
import type {FC} from "react";
import type {RootState} from "../../store";
import {useNavigate} from "react-router-dom";

interface HeaderProps {
    onMenuToggle?: () => void;
    isSidebarOpen?: boolean;
}


export const Header: FC<HeaderProps> = ({onMenuToggle, isSidebarOpen}) => {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const refreshToken = useSelector((s: RootState) => s.auth.refreshToken);

    const handleServerLogout = async () => {
        await logoutServer(refreshToken, dispatch);
        navigate("/login", { replace: true });
    };
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
                <Button appearance="primary" onClick={handleServerLogout}>Выйти</Button>
            </div>
        </header>
    );
};
