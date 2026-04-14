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
import {useGetTariffsPageQuery, useGetAllPromotionsQuery, useGetVisitsPageQuery} from "@store/api/venueApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {useComponentSize} from "@hooks/useComponentSize";

interface StatCardProps {
    title: string;
    value: string | number;
    icon: React.ReactElement;
    onClick?: () => void;
}

const StatCard = ({title, value, icon, onClick, cardSize}: StatCardProps & {cardSize: "small" | "medium" | "large"}) => (
    <Card
        className={`flex-1 min-w-[200px] ${onClick ? "cursor-pointer" : ""}`}
        size={cardSize}
        onClick={onClick}
    >
        <div className="flex items-center justify-between">
            <div className="min-w-0 flex-1">
                <Body2 block className="line-clamp-3">{title}</Body2>
                <Title2>{value}</Title2>
            </div>
            <div className="text-2xl opacity-50 shrink-0 ml-2">{icon}</div>
        </div>
    </Card>
);

export const DashboardPage = () => {
    const navigate = useNavigate();
    const {sizes} = useComponentSize();
    const {data: usersData, isLoading: usersLoading, error: usersError} = useGetUsersQuery({page: 1, size: 1});
    const {data: tariffsData, isLoading: tariffsLoading} = useGetTariffsPageQuery({pageNumber: 1, pageSize: 1});
    const {data: promotionsData, isLoading: promotionsLoading} = useGetAllPromotionsQuery();
    const {data: visitsData, isLoading: visitsLoading} = useGetVisitsPageQuery({pageNumber: 1, pageSize: 1});
    const errorMessage = usersError ? getRtkErrorMessage(usersError as FetchBaseQueryError) : null;

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
                    value={usersLoading ? "..." : (usersData?.pagination.totalCount ?? "—")}
                    icon={<People20Regular />}
                    onClick={() => navigate("/admin/users")}
                    cardSize={sizes.card}
                />
                <StatCard
                    title="Тарифы"
                    value={tariffsLoading ? "..." : (tariffsData?.totalCount ?? "—")}
                    icon={<Money20Regular />}
                    onClick={() => navigate("/admin/tariffs")}
                    cardSize={sizes.card}
                />
                <StatCard
                    title="Визиты"
                    value={visitsLoading ? "..." : (visitsData?.totalCount ?? "—")}
                    icon={<Clock20Regular />}
                    onClick={() => navigate("/admin/visits")}
                    cardSize={sizes.card}
                />
                <StatCard
                    title="Акции"
                    value={promotionsLoading ? "..." : (promotionsData?.length ?? "—")}
                    icon={<Gift20Regular />}
                    onClick={() => navigate("/admin/promotions")}
                    cardSize={sizes.card}
                />
            </div>

            <div className="flex gap-4 flex-wrap">
                <Card className="flex-1 min-w-[300px]" size={sizes.card}>
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

                <Card className="flex-1 min-w-[300px]" size={sizes.card}>
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
