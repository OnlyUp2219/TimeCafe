import {Hamburger, Button, Avatar} from "@fluentui/react-components";
import {logoutServer} from "../../api/auth.ts";
import {useDispatch} from "react-redux";
import type {FC} from "react";
import {useNavigate} from "react-router-dom";

interface HeaderProps {
    onMenuToggle?: () => void;
    isSidebarOpen?: boolean;
    variant?: "app" | "public";
}


export const Header: FC<HeaderProps> = ({onMenuToggle, isSidebarOpen, variant = "app"}) => {
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const handleServerLogout = async () => {
        await logoutServer(dispatch);
        navigate("/login", { replace: true });
    };

    const isPublic = variant === "public";
    return (
        <header className="w-full border-b border-slate-200 bg-white/90 backdrop-blur supports-[backdrop-filter]:bg-white/70">
            <div className="mx-auto flex max-w-6xl items-center justify-between gap-3 px-4 py-3 sm:px-6">
                <div className="flex items-center gap-3">
                    {!isPublic && !isSidebarOpen && onMenuToggle && (
                    <Hamburger
                        aria-label="Toggle sidebar"
                        onClick={onMenuToggle}
                    />
                )}
                    <h1 className="text-lg font-semibold tracking-tight text-slate-900 sm:text-xl">TimeCafe</h1>
                </div>

                {isPublic ? (
                    <div className="flex items-center gap-2">
                        <Button appearance="secondary" onClick={() => navigate("/login")}>Войти</Button>
                        <Button appearance="primary" onClick={() => navigate("/register")}>Регистрация</Button>
                    </div>
                ) : (
                    <div className="flex items-center gap-3">
                        <Avatar/>
                        <Button appearance="primary" onClick={handleServerLogout}>Выйти</Button>
                    </div>
                )}
            </div>
        </header>
    );
};
