import {useMemo} from "react";
import {useNavigate, useLocation} from "react-router-dom";
import {useSelector} from "react-redux";
import type {RootState} from "../../store";
import {EmailPendingCard} from "../../components/EmailPendingCard/EmailPendingCard";
import {Button} from "@fluentui/react-components";

type LocationState = {
    mockLink?: string;
} | null;

export const EmailPendingPage = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const email = useSelector((s: RootState) => s.auth.email);

    const mockLink = useMemo(() => {
        const state = location.state as LocationState;
        return state?.mockLink;
    }, [location.state]);

    if (!email) {
        return (
            <div className="flex flex-col gap-4 items-center justify-center">
                <div className="text-center">Нет данных о почте. Перейдите к регистрации или входу.</div>
                <div className="flex gap-2">
                    <Button appearance="primary" onClick={() => navigate("/register")}>Регистрация</Button>
                    <Button appearance="secondary" onClick={() => navigate("/login")}>Вход</Button>
                </div>
            </div>
        );
    }

    return (
        <div className="flex items-center justify-center">
            <EmailPendingCard
                mockLink={mockLink}
                onGoToLogin={() => navigate("/login")}
            />
        </div>
    );
};
