import {
    Card,
    Title2,
    Body1,
    Body2,
    Badge,
    MessageBar,
    MessageBarBody,
} from "@fluentui/react-components";
import {
    People20Regular,
    Money20Regular,
    Clock20Regular,
    Gift20Regular,
} from "@fluentui/react-icons";
import { useNavigate } from "react-router-dom";
import { useGetUsersQuery } from "@store/api/adminApi";
import { useGetVisitsPageQuery, useGetPendingVisitsQuery } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useComponentSize } from "@hooks/useComponentSize";
import { KpiCard } from "@components/Admin/KpiCard";


export const DashboardPage = () => {


    const navigate = useNavigate();
    const { sizes } = useComponentSize();
    const { data: usersData, isLoading: usersLoading, error: usersError } = useGetUsersQuery({ page: 1, size: 1 });
    const { data: visitsData, isLoading: visitsLoading } = useGetVisitsPageQuery({ page: 1, pageSize: 1 });
    const { data: pendingData, isLoading: pendingLoading } = useGetPendingVisitsQuery({ page: 1, pageSize: 1 });
    const errorMessage = usersError ? getRtkErrorMessage(usersError as FetchBaseQueryError) : null;

    return (
        <div>
            <div className="flex flex-col gap-1">
                <Title2>Дашборд</Title2>
                <Body1 className="mt-1">Обзор системы TimeCafe</Body1>
            </div>

            {errorMessage && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{errorMessage}</MessageBarBody>
                </MessageBar>
            )}

            <div className="flex gap-4 flex-wrap mb-6">
                <KpiCard
                    title="Пользователи"
                    value={usersLoading ? "..." : (usersData?.metadata.totalCount ?? "—")}
                    icon={<People20Regular />}
                    onClick={() => navigate("/admin/users")}
                />
                <KpiCard
                    title="Визиты"
                    value={visitsLoading ? "..." : (visitsData?.metadata?.totalCount ?? "—")}
                    icon={<Clock20Regular />}
                    onClick={() => navigate("/admin/visits")}
                />
                <KpiCard
                    title="Ожидают подтверждения"
                    value={pendingLoading ? "..." : (pendingData?.metadata?.totalCount ?? "—")}
                    icon={<Gift20Regular />}
                    onClick={() => navigate("/admin/visits/pending")}
                />
                <KpiCard
                    title="Выручка (₽)"
                    value="—"
                    icon={<Money20Regular />}
                    onClick={() => navigate("/admin/payments")}
                />
            </div>

            <div className="flex flex-col gap-4 flex-wrap mb-6">
                <Card className="flex-1 min-w-[300px]" size={sizes.card}>
                    <Title2 className="mb-2">Визиты за 24ч (Grafana)</Title2>
                    <iframe
                        src="http://localhost:3000/d-solo/timecafe/overview?panelId=7&theme=light"
                        width="100%"
                        height="400"
                        style={{ border: "none", borderRadius: "var(--borderRadiusXLarge)" }}
                        title="Grafana Visits"
                    />
                </Card>

                <Card className="flex-1 min-w-[300px]" size={sizes.card}>
                    <Title2 className="mb-2">HTTP Latency P95 (Grafana)</Title2>
                    <iframe
                        src="http://localhost:3000/d-solo/timecafe/overview?panelId=5&theme=light"
                        width="100%"
                        height="400"
                        style={{ border: "none", borderRadius: "var(--borderRadiusXLarge)" }}
                        title="Grafana Latency"
                    />
                </Card>

                <Card className="flex-1 min-w-[300px]" size={sizes.card}>
                    <Title2 className="mb-2">Последние ошибки (Kibana)</Title2>
                    <iframe
                        src="http://localhost:5601/app/discover#/?_g=(filters:!(),refreshInterval:(pause:!t,value:0),time:(from:now-24h,to:now))&_a=(columns:!(),filters:!(),index:'*',interval:auto,query:(language:kuery,query:'level:%20error'),sort:!(!('@timestamp',desc)))"
                        width="100%"
                        height="400"
                        style={{ border: "none", borderRadius: "var(--borderRadiusXLarge)" }}
                        title="Kibana Errors"
                    />
                </Card>
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
                            onClick={() => navigate("/admin/visits/pending")}
                        >
                            → Ожидающие визиты
                        </Body1>
                        <Body1
                            className="cursor-pointer hover:underline"
                            onClick={() => navigate("/admin/tariffs")}
                        >
                            → Управление тарифами
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
