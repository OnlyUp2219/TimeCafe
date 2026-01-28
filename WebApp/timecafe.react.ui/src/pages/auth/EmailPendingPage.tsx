import {useMemo} from "react";
import {useNavigate, useLocation} from "react-router-dom";
import {useSelector} from "react-redux";
import type {RootState} from "../../store";
import {EmailPendingCard} from "../../components/EmailPendingCard/EmailPendingCard";
import {Body2, Title3} from "@fluentui/react-components";
import {TooltipButton} from "../../components/TooltipButton/TooltipButton";
import {authFormContainerClassName} from "../../layouts/authLayout";

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
            <div
                className="!grid grid-cols-1 items-center justify-center
             sm:grid-cols-2 sm:justify-stretch sm:items-stretch">
                <div id="Left Side" className="relative hidden sm:block bg-[url(/src/assets/abstract_bg.svg)] bg-left bg-cover bg-no-repeat">
                    <div className="absolute inset-0 bg-black/40 pointer-events-none" />
                </div>

                <div id="Form" className={authFormContainerClassName}>
                    <div className="flex flex-col w-full max-w-md gap-[12px]">
                        <div className="flex flex-col items-center">
                            <Title3 block>Подтверждение email</Title3>
                            <Body2 block>Нет данных о почте. Перейдите к регистрации или входу.</Body2>
                        </div>

                        <div className="grid grid-cols-1 gap-[12px] sm:grid-cols-2">
                            <TooltipButton
                                appearance="primary"
                                onClick={() => navigate("/register")}
                                tooltip="Перейти к регистрации"
                                label="Регистрация"
                            />
                            <TooltipButton
                                appearance="secondary"
                                onClick={() => navigate("/login")}
                                tooltip="Перейти к входу"
                                label="Вход"
                            />
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div
            className="!grid grid-cols-1 items-center justify-center
             sm:grid-cols-2 sm:justify-stretch sm:items-stretch">
            <div id="Left Side" className="relative hidden sm:block bg-[url(/src/assets/abstract_bg.svg)] bg-left bg-cover bg-no-repeat">
                <div className="absolute inset-0 bg-black/40 pointer-events-none" />
            </div>

            <div id="Form" className={authFormContainerClassName}>
                <div className="flex flex-col w-full max-w-md gap-[12px]">
                    <EmailPendingCard
                        mockLink={mockLink}
                        onGoToLogin={() => navigate("/login")}
                    />
                </div>
            </div>
        </div>
    );
};
