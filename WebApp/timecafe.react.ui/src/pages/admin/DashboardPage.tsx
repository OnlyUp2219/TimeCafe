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
import { useGetUsersQuery, useGetSystemStatusQuery } from "@store/api/adminApi";
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
    const { data: systemStatus, isLoading: systemStatusLoading } = useGetSystemStatusQuery(undefined, { pollingInterval: 10000 });
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

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                <Card className="p-4 cursor-pointer" size={sizes.card} onClick={() => navigate("/admin/monitoring/grafana")}>
                    <div className="flex justify-between items-center mb-2">
                        <Title2>Мониторинг Grafana</Title2>
                        <Body2 className="text-[var(--colorBrandForegroundLink)] hover:underline">Открыть графики →</Body2>
                    </div>
                    <Body1 className="text-[var(--colorNeutralForeground2)]">
                        Просмотр графиков активности визитов, задержки HTTP-запросов и общей производительности системы.
                    </Body1>
                </Card>

                <Card className="p-4 cursor-pointer" size={sizes.card} onClick={() => navigate("/admin/monitoring/kibana")}>
                    <div className="flex justify-between items-center mb-2">
                        <Title2>Логи Kibana</Title2>
                        <Body2 className="text-[var(--colorBrandForegroundLink)] hover:underline">Открыть логи →</Body2>
                    </div>
                    <Body1 className="text-[var(--colorNeutralForeground2)]">
                        Анализ последних технических ошибок системы, логов и трассировок по всем микросервисам.
                    </Body1>
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
                            {systemStatusLoading ? (
                                <Badge appearance="filled" color="warning">Checking...</Badge>
                            ) : (
                                <Badge appearance="filled" color={systemStatus?.Auth === "Online" ? "success" : "danger"}>
                                    {systemStatus?.Auth ?? "Offline"}
                                </Badge>
                            )}
                        </div>
                        <div className="flex items-center justify-between">
                            <Body1>Venue Service</Body1>
                            {systemStatusLoading ? (
                                <Badge appearance="filled" color="warning">Checking...</Badge>
                            ) : (
                                <Badge appearance="filled" color={systemStatus?.Venue === "Online" ? "success" : "danger"}>
                                    {systemStatus?.Venue ?? "Offline"}
                                </Badge>
                            )}
                        </div>
                        <div className="flex items-center justify-between">
                            <Body1>Billing Service</Body1>
                            {systemStatusLoading ? (
                                <Badge appearance="filled" color="warning">Checking...</Badge>
                            ) : (
                                <Badge appearance="filled" color={systemStatus?.Billing === "Online" ? "success" : "danger"}>
                                    {systemStatus?.Billing ?? "Offline"}
                                </Badge>
                            )}
                        </div>
                        <div className="flex items-center justify-between">
                            <Body1>UserProfile Service</Body1>
                            {systemStatusLoading ? (
                                <Badge appearance="filled" color="warning">Checking...</Badge>
                            ) : (
                                <Badge appearance="filled" color={systemStatus?.UserProfile === "Online" ? "success" : "danger"}>
                                    {systemStatus?.UserProfile ?? "Offline"}
                                </Badge>
                            )}
                        </div>
                    </div>
                </Card>
            </div>
        </div>
    );
};
