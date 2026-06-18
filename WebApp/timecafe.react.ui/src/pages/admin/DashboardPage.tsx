import {
    Card,
    Title2,
    Body1,
    Body2,
    Badge,
} from "@fluentui/react-components";
import {
    People20Regular,
    Money20Regular,
    Clock20Regular,
    Gift20Regular,
} from "@fluentui/react-icons";
import { useNavigate } from "react-router-dom";
import { useGetUsersQuery, useGetSystemStatusQuery } from "@store/api/adminApi";
import { useGetVisitsPageQuery, useGetPendingVisitsQuery, useGetActiveVisitsQuery } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useComponentSize } from "@hooks/useComponentSize";
import { KpiCard } from "@components/Admin/KpiCard";
import { DismissableError } from "@components/DismissableError/DismissableError";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions } from "@shared/auth/permissions";
import { NO_DATA } from "@shared/const/placeholders";

export const DashboardPage = () => {
    const navigate = useNavigate();
    const { sizes } = useComponentSize();

    const { data: usersData, isLoading: usersLoading, error: usersError } = useGetUsersQuery({ page: 1, size: 1 }, { pollingInterval: 5000 });
    const { data: visitsData, isLoading: visitsLoading } = useGetVisitsPageQuery({ page: 1, pageSize: 1 }, { pollingInterval: 5000 });
    const { data: pendingData, isLoading: pendingLoading } = useGetPendingVisitsQuery({ page: 1, pageSize: 1 }, { pollingInterval: 5000 });
    const { data: activeVisitsData, isLoading: activeVisitsLoading } = useGetActiveVisitsQuery(undefined, { pollingInterval: 5000 });
    const { data: systemStatus, isLoading: systemStatusLoading } = useGetSystemStatusQuery(undefined, { pollingInterval: 10000 });
    const errorMessage = usersError ? getRtkErrorMessage(usersError as FetchBaseQueryError) : null;

    const usersValue = usersLoading ? NO_DATA : (usersData?.metadata?.totalCount ?? NO_DATA);
    const visitsValue = visitsLoading ? NO_DATA : (visitsData?.metadata?.totalCount ?? NO_DATA);
    const pendingValue = pendingLoading ? NO_DATA : (pendingData?.metadata?.totalCount ?? NO_DATA);
    const finishRequestedCount = activeVisitsData?.filter(v => v.isFinishRequested).length ?? 0;
    const finishRequestedValue = activeVisitsLoading ? NO_DATA : finishRequestedCount;

    const isAuthOnline = systemStatus?.Auth === "Online";
    const isVenueOnline = systemStatus?.Venue === "Online";
    const isBillingOnline = systemStatus?.Billing === "Online";
    const isUserProfileOnline = systemStatus?.UserProfile === "Online";

    return (
        <div className="flex flex-col gap-2">
            <div className="flex flex-col">
                <Title2>Дашборд</Title2>
                <Body1>Обзор системы TimeCafe</Body1>
            </div>

            <DismissableError error={errorMessage} />

            <div className="flex gap-4 flex-wrap">
                <HasPermission can={Permissions.AccountAdminRead}>
                    <KpiCard
                        title="Пользователи"
                        value={usersValue}
                        icon={<People20Regular />}
                        onClick={() => navigate("/admin/users")}
                    />
                </HasPermission>
                <HasPermission can={Permissions.VenueVisitRead}>
                    <KpiCard
                        title="Визиты"
                        value={visitsValue}
                        icon={<Clock20Regular />}
                        onClick={() => navigate("/admin/visits")}
                    />
                </HasPermission>
                <HasPermission can={Permissions.VenueVisitViewPending}>
                    <KpiCard
                        title="Ожидают подтверждения"
                        value={pendingValue}
                        icon={<Gift20Regular />}
                        onClick={() => navigate("/admin/visits/pending")}
                    />
                </HasPermission>
                <HasPermission can={Permissions.VenueVisitRead}>
                    <KpiCard
                        title="Запросы на выход"
                        value={finishRequestedValue}
                        icon={<Clock20Regular />}
                        onClick={() => navigate("/admin/visits")}
                    />
                </HasPermission>
                <HasPermission can={Permissions.BillingPaymentHistoryRead}>
                    <KpiCard
                        title="Выручка (₽)"
                        value={NO_DATA}
                        icon={<Money20Regular />}
                        onClick={() => navigate("/admin/payments")}
                    />
                </HasPermission>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <HasPermission can={Permissions.AuditLogAdminRead}>
                    <Card className="p-4 cursor-pointer" size={sizes.card} onClick={() => navigate("/admin/monitoring/grafana")}>
                        <div className="flex justify-between items-center mb-2">
                            <Title2>Мониторинг Grafana</Title2>
                            <Body2 className="text-(--colorBrandForegroundLink) hover:underline">Открыть графики →</Body2>
                        </div>
                        <Body1 className="text-(--colorNeutralForeground2)">
                            Просмотр графиков активности визитов, задержки HTTP-запросов и общей производительности системы.
                        </Body1>
                    </Card>
                </HasPermission>

                <HasPermission can={Permissions.AuditLogAdminRead}>
                    <Card className="p-4 cursor-pointer" size={sizes.card} onClick={() => navigate("/admin/monitoring/kibana")}>
                        <div className="flex justify-between items-center mb-2">
                            <Title2>Логи Kibana</Title2>
                            <Body2 className="text-(--colorBrandForegroundLink) hover:underline">Открыть логи →</Body2>
                        </div>
                        <Body1 className="text-(--colorNeutralForeground2)">
                            Анализ последних технических ошибок системы, логов и трассировок по всем микросервисам.
                        </Body1>
                    </Card>
                </HasPermission>
            </div>

            <div className="flex gap-4 flex-wrap">
                <Card className="flex-1 min-w-[300px]" size={sizes.card}>
                    <Title2 className="mb-2">Быстрые действия</Title2>
                    <div className="flex flex-col gap-2">
                        <HasPermission can={Permissions.AccountAdminRead}>
                            <Body1
                                className="cursor-pointer hover:underline"
                                onClick={() => navigate("/admin/users")}
                            >
                                → Управление пользователями
                            </Body1>
                        </HasPermission>
                        <HasPermission can={Permissions.VenueVisitViewPending}>
                            <Body1
                                className="cursor-pointer hover:underline"
                                onClick={() => navigate("/admin/visits/pending")}
                            >
                                → Ожидающие визиты
                            </Body1>
                        </HasPermission>
                        <HasPermission can={Permissions.VenueTariffRead}>
                            <Body1
                                className="cursor-pointer hover:underline"
                                onClick={() => navigate("/admin/tariffs")}
                            >
                                → Управление тарифами
                            </Body1>
                        </HasPermission>
                    </div>
                </Card>

                <Card className="flex-1 min-w-[300px]" size={sizes.card}>
                    <Title2 className="mb-2">Статус системы</Title2>
                    <div className="flex flex-col gap-2">
                        <div className="flex items-center justify-between">
                            <Body1>Auth Service</Body1>
                            {systemStatusLoading && (
                                <Badge appearance="filled" color="warning">Checking...</Badge>
                            )}
                            {!systemStatusLoading && (
                                <Badge appearance="filled" color={isAuthOnline ? "success" : "danger"}>
                                    {systemStatus?.Auth ?? "Offline"}
                                </Badge>
                            )}
                        </div>
                        <div className="flex items-center justify-between">
                            <Body1>Venue Service</Body1>
                            {systemStatusLoading && (
                                <Badge appearance="filled" color="warning">Checking...</Badge>
                            )}
                            {!systemStatusLoading && (
                                <Badge appearance="filled" color={isVenueOnline ? "success" : "danger"}>
                                    {systemStatus?.Venue ?? "Offline"}
                                </Badge>
                            )}
                        </div>
                        <div className="flex items-center justify-between">
                            <Body1>Billing Service</Body1>
                            {systemStatusLoading && (
                                <Badge appearance="filled" color="warning">Checking...</Badge>
                            )}
                            {!systemStatusLoading && (
                                <Badge appearance="filled" color={isBillingOnline ? "success" : "danger"}>
                                    {systemStatus?.Billing ?? "Offline"}
                                </Badge>
                            )}
                        </div>
                        <div className="flex items-center justify-between">
                            <Body1>UserProfile Service</Body1>
                            {systemStatusLoading && (
                                <Badge appearance="filled" color="warning">Checking...</Badge>
                            )}
                            {!systemStatusLoading && (
                                <Badge appearance="filled" color={isUserProfileOnline ? "success" : "danger"}>
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

