import {
    Card,
    Title2,
    Body1,
    Body2,
    Badge,
    Spinner,
    MessageBar,
    MessageBarBody,
} from "@fluentui/react-components";
import {
    People20Regular,
    Money20Regular,
    Clock20Regular,
    Gift20Regular,
} from "@fluentui/react-icons";
import {useNavigate} from "react-router-dom";
import {useGetUsersQuery} from "@store/api/adminApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";

interface StatCardProps {
    title: string;
    value: string | number;
    icon: React.ReactElement;
    onClick?: () => void;
}

const StatCard = ({title, value, icon, onClick}: StatCardProps) => (
    <Card
        className={`flex-1 min-w-[200px] ${onClick ? "cursor-pointer" : ""}`}
        size="large"
        onClick={onClick}
    >
        <div className="flex items-center justify-between">
            <div>
                <Body2 block>{title}</Body2>
                <Title2>{value}</Title2>
            </div>
            <div className="text-2xl opacity-50">{icon}</div>
        </div>
    </Card>
);

export const DashboardPage = () => {
    const navigate = useNavigate();
    const {data: usersData, isLoading, error} = useGetUsersQuery({page: 1, size: 1});
    const errorMessage = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    return (
        <div>
            <div className="mb-6">
                <Title2>Дашборд</Title2>
                <Body1 block className="mt-1">Обзор системы TimeCafe</Body1>
            </div>

            {errorMessage && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{errorMessage}</MessageBarBody>
                </MessageBar>
            )}

            <div className="flex gap-4 flex-wrap mb-6">
                <StatCard
                    title="Пользователи"
                    value={isLoading ? "..." : (usersData?.pagination.totalCount ?? "—")}
                    icon={<People20Regular />}
                    onClick={() => navigate("/admin/users")}
                />
                <StatCard
                    title="Тарифы"
                    value="—"
                    icon={<Money20Regular />}
                    onClick={() => navigate("/admin/tariffs")}
                />
                <StatCard
                    title="Визиты"
                    value="—"
                    icon={<Clock20Regular />}
                    onClick={() => navigate("/admin/visits")}
                />
                <StatCard
                    title="Акции"
                    value="—"
                    icon={<Gift20Regular />}
                    onClick={() => navigate("/admin/promotions")}
                />
            </div>

            <div className="flex gap-4 flex-wrap">
                <Card className="flex-1 min-w-[300px]" size="large">
                    <Title2 className="mb-2">Быстрые действия</Title2>
                    <div className="flex flex-col gap-2">
                        <Body1
                            className="cursor-pointer hover:underline"
                            onClick={() => navigate("/admin/users")}
                        >
                            → Управление пользователями
                        </Body1>
                        <Body1
                            className="cursor-pointer hover:underline"
                            onClick={() => navigate("/admin/tariffs")}
                        >
                            → Управление тарифами
                        </Body1>
                        <Body1
                            className="cursor-pointer hover:underline"
                            onClick={() => navigate("/admin/promotions")}
                        >
                            → Управление акциями
                        </Body1>
                    </div>
                </Card>

                <Card className="flex-1 min-w-[300px]" size="large">
                    <Title2 className="mb-2">Статус системы</Title2>
                    <div className="flex flex-col gap-2">
                        <div className="flex items-center justify-between">
                            <Body1>Auth Service</Body1>
                            <Badge appearance="filled" color="success">Online</Badge>
                        </div>
                        <div className="flex items-center justify-between">
                            <Body1>Venue Service</Body1>
                            <Badge appearance="filled" color="success">Online</Badge>
                        </div>
                        <div className="flex items-center justify-between">
                            <Body1>Billing Service</Body1>
                            <Badge appearance="filled" color="success">Online</Badge>
                        </div>
                        <div className="flex items-center justify-between">
                            <Body1>UserProfile Service</Body1>
                            <Badge appearance="filled" color="success">Online</Badge>
                        </div>
                    </div>
                </Card>
            </div>
        </div>
    );
};
