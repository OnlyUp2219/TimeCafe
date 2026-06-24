import { NO_DATA } from "@shared/const/placeholders";
import { useMemo, useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
    Body1,
    Body2,
    Button,
    Card,
    Title2,
    createTableColumn,
    TableCellLayout,
    Badge,
    Tooltip,
    Tab,
    TabList,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { Eye20Regular, Clock20Regular, Warning20Regular } from "@fluentui/react-icons";
import { DismissableError } from "@components/DismissableError/DismissableError";
import { useGetVisitsPageQuery, useApproveVisitMutation, useRejectVisitMutation } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import type { VisitWithTariff } from "@app-types/visitWithTariff";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { VisitStatusBadge } from "@components/VisitStatusBadge";
import { useComponentSize } from "@hooks/useComponentSize";
import { usePermissions } from "@hooks/usePermissions";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions, type Permission } from "@shared/auth/permissions";
import { ApproveVisitDialog } from "@components/Admin/ApproveVisitDialog/ApproveVisitDialog";
import { UserCell } from "@components/DataTable/cells/UserCell";
import { useGetBalancesBulkQuery } from "@store/api/billingApi";
import { VisitStatus } from "@app-types/visit";
import { usePagination } from "@hooks/usePagination";
import { WalkInVisitDialog } from "@components/Admin/WalkInVisitDialog/WalkInVisitDialog";
import { RequirePermission } from "@app/components/RequirePermission/RequirePermission";
import { PageLoader } from "@components/PageLoader/PageLoader";
import { formatDateTime } from "@utility/dateUtils";
import { formatDurationMinutes } from "@utility/formatDurationMinutes";
import { formatMoney } from "@utility/formatUtils";

const formatCost = (cost: number | null, status: VisitStatus) => {
    if (status === VisitStatus.Cancelled || status === VisitStatus.Rejected) {
        return formatMoney(0);
    }
    return formatMoney(cost);
};const getVisitDurationMs = (visit: VisitWithTariff, now: Date) => {
    if (visit.status === VisitStatus.Pending || 
        visit.status === VisitStatus.Approved || 
        visit.status === VisitStatus.Rejected || 
        visit.status === VisitStatus.Cancelled) {
        return 0;
    }
    const start = new Date(visit.entryTime).getTime();
    const end = visit.exitTime ? new Date(visit.exitTime).getTime() : now.getTime();
    return Math.max(0, end - start);
};
export const VisitsPage = () => {
    const navigate = useNavigate();
    const { sizes } = useComponentSize();
    const { has } = usePermissions();
    const { page: currentPage, size: pageSize, setPage: setCurrentPage, setSize: setPageSize } = usePagination("adminVisits");

    const [currentTime, setCurrentTime] = useState(() => new Date());

    useEffect(() => {
        const timer = setInterval(() => {
            setCurrentTime(new Date());
        }, 30000);
        return () => clearInterval(timer);
    }, []);

    const [selectedVisit, setSelectedVisit] = useState<VisitWithTariff | null>(null);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [walkInOpen, setWalkInOpen] = useState(false);
    const [actionError, setActionError] = useState<string | null>(null);
    const [selectedTab, setSelectedTab] = useState<"all" | "pending" | "finishRequested" | "active">("all");

    const [approveVisit, { isLoading: approving }] = useApproveVisitMutation();
    const [rejectVisit, { isLoading: rejecting }] = useRejectVisitMutation();

    const { data, isLoading, error, refetch } = useGetVisitsPageQuery(
        { page: currentPage, pageSize },
        { refetchOnMountOrArgChange: true, pollingInterval: 5000 }
    );

    const handleApprove = async (visitId: string) => {
        setActionError(null);
        try {
            await approveVisit(visitId).unwrap();
            setDialogOpen(false);
            void refetch();
        } catch (err) {
            setActionError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось подтвердить визит");
        }
    };

    const handleReject = async (visitId: string, reason: string) => {
        setActionError(null);
        try {
            await rejectVisit({ visitId, reason }).unwrap();
            setDialogOpen(false);
            void refetch();
        } catch (err) {
            setActionError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось отклонить визит");
        }
    };

    const openDialog = (visit: VisitWithTariff) => {
        setSelectedVisit(visit);
        setActionError(null);
        setDialogOpen(true);
    };
    const visits = data?.items ?? [];
    const filteredVisits = useMemo(() => {
        if (selectedTab === "all") return visits;
        if (selectedTab === "pending") return visits.filter(v => v.status === VisitStatus.Pending);
        if (selectedTab === "finishRequested") return visits.filter(v => v.status === VisitStatus.Active && v.isFinishRequested);
        if (selectedTab === "active") return visits.filter(v => v.status === VisitStatus.Active);
        return visits;
    }, [visits, selectedTab]);
    const totalCount = data?.metadata?.totalCount ?? 0;
    const totalPages = data?.metadata?.totalPages ?? 1;
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const userIds = useMemo(() => [...new Set(visits.map(v => v.userId).filter(Boolean) as string[])], [visits]);
    const { data: balances } = useGetBalancesBulkQuery(userIds, { skip: userIds.length === 0 });

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        tariff: { minWidth: 120, defaultWidth: 200 },
        status: { minWidth: 140, defaultWidth: 200 },
        entryTime: { minWidth: 130, defaultWidth: 170 },
        exitTime: { minWidth: 130, defaultWidth: 170 },
        duration: { minWidth: 100, defaultWidth: 150 },
        cost: { minWidth: 80, defaultWidth: 120 },
        userId: { minWidth: 100, defaultWidth: 180 },
        warnings: { minWidth: 150, defaultWidth: 250 },
    }), []);

    const columns: TableColumnDefinition<VisitWithTariff>[] = useMemo(() => {
        const allColumns: (TableColumnDefinition<VisitWithTariff> & { permission?: string })[] = [
            createTableColumn<VisitWithTariff>({
                columnId: "tariff",
                compare: (a, b) => a.tariffName.localeCompare(b.tariffName),
                renderHeaderCell: () => "Тариф",
                renderCell: (visit) => (
                    <TableCellLayout truncate>
                        <Body1>{visit.tariffName || NO_DATA}</Body1>
                    </TableCellLayout>
                ),
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "status",
                compare: (a, b) => a.status - b.status,
                renderHeaderCell: () => "Статус",
                renderCell: (visit) => (
                    <TableCellLayout truncate>
                        <div className="flex items-center gap-2">
                            <VisitStatusBadge status={visit.status} />
                            {visit.isFinishRequested && (
                                <Badge color="danger" appearance="filled" size="small" title="Запрошен выход">
                                    🚨
                                </Badge>
                            )}
                        </div>
                    </TableCellLayout>
                ),
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "entryTime",
                compare: (a, b) => new Date(a.entryTime).getTime() - new Date(b.entryTime).getTime(),
                renderHeaderCell: () => "Вход",
                renderCell: (visit) => (
                    <TableCellLayout truncate>{formatDateTime(visit.entryTime)}</TableCellLayout>
                ),
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "exitTime",
                compare: (a, b) => new Date(a.exitTime ?? "").getTime() - new Date(b.exitTime ?? "").getTime(),
                renderHeaderCell: () => "Выход",
                renderCell: (visit) => (
                    <TableCellLayout truncate>
                        {visit.status === VisitStatus.Completed ? formatDateTime(visit.exitTime) : NO_DATA}
                    </TableCellLayout>
                ),
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "duration",
                compare: (a, b) => {
                    const durationA = getVisitDurationMs(a, currentTime);
                    const durationB = getVisitDurationMs(b, currentTime);
                    return durationA - durationB;
                },
                renderHeaderCell: () => "Время проведения",
                renderCell: (visit) => {
                    const ms = getVisitDurationMs(visit, currentTime);
                    const minutes = Math.floor(ms / 60000);
                    return <TableCellLayout truncate>{formatDurationMinutes(minutes)}</TableCellLayout>;
                },
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "cost",
                compare: (a, b) => (a.calculatedCost ?? 0) - (b.calculatedCost ?? 0),
                renderHeaderCell: () => "Стоимость",
                renderCell: (visit) => (
                    <TableCellLayout truncate>{formatCost(visit.calculatedCost, visit.status)}</TableCellLayout>
                ),
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "userId",
                compare: (a, b) => (a.userId ?? "").localeCompare(b.userId ?? ""),
                renderHeaderCell: () => "Пользователь",
                renderCell: (visit) => <UserCell userId={visit.userId} />,
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "actions",
                compare: () => 0,
                renderHeaderCell: () => "Действия",
                renderCell: (visit) => (
                    <HasPermission can={Permissions.VenueVisitRead}>
                        <Button
                            appearance="subtle"
                            size={sizes.button}
                            icon={<Eye20Regular />}
                            onClick={() => {
                                if (visit.status === VisitStatus.Pending) {
                                    openDialog(visit);
                                } else {
                                    navigate(`/admin/visits/${visit.visitId}`);
                                }
                            }}
                        >
                            Открыть
                        </Button>
                    </HasPermission>
                ),
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "warnings",
                compare: () => 0,
                renderHeaderCell: () => "Предупреждения",
                renderCell: (visit) => {
                    const warnings: string[] = [];
                    if (visit.resourceMaxGuests != null && visit.guestsCount > visit.resourceMaxGuests) {
                        warnings.push(`Вместимость (макс: ${visit.resourceMaxGuests})`);
                    }
                    if (visit.userId && balances && balances[visit.userId] != null) {
                        const balance = balances[visit.userId];
                        if (balance < 0 || (visit.status === VisitStatus.Active && balance < (visit.calculatedCost ?? 0))) {
                            warnings.push(`Долг/Меньше цены (${formatMoney(balance)})`);
                        }
                    }
                    if (warnings.length === 0) return <TableCellLayout truncate>{NO_DATA}</TableCellLayout>;
                    return (
                        <TableCellLayout>
                            <Tooltip content={warnings.join("\n")} relationship="label">
                                <span className="flex items-center text-(--colorPaletteRedForeground1)">
                                    <Warning20Regular />
                                </span>
                            </Tooltip>
                        </TableCellLayout>
                    );
                },
            }),
        ];

        return allColumns.filter(col => !col.permission || has(col.permission as Permission));
    }, [navigate, has, balances, currentTime]);

    if (isLoading) {
        return <PageLoader label="Загрузка визитов..." />;
    }

    return (
        <RequirePermission can={Permissions.VenueVisitRead}>
            <div className="flex flex-col gap-2">
                <div className="flex items-center justify-between flex-wrap gap-4">
                    <div className="flex flex-col">
                        <Title2>Визиты</Title2>
                        <Body2>{totalCount} визитов</Body2>
                    </div>
                    <div className="flex gap-2">
                        <HasPermission can={Permissions.VenueVisitCreate}>
                            <Button
                                appearance="primary"
                                size={sizes.button}
                                onClick={() => setWalkInOpen(true)}
                            >
                                Быстрая посадка (Walk-in)
                            </Button>
                        </HasPermission>
                        <HasPermission can={Permissions.VenueVisitViewPending}>
                            <Button
                                appearance="outline"
                                size={sizes.button}
                                icon={<Clock20Regular />}
                                onClick={() => navigate("/admin/visits/pending")}
                            >
                                Ожидают подтверждения
                            </Button>
                        </HasPermission>
                    </div>
                </div>

                <DismissableError error={queryError} />
                <DismissableError error={actionError} />

                <TabList selectedValue={selectedTab} onTabSelect={(_, data) => setSelectedTab(data.value as any)}>
                    <Tab value="all">Все</Tab>
                    <Tab value="pending">Ожидают входа</Tab>
                    <Tab value="active">Активные</Tab>
                    <Tab value="finishRequested">Запрос на выход</Tab>
                </TabList>

                <Card className="overflow-x-auto" size={sizes.card}>
                    <DataTable
                        items={filteredVisits}
                        columns={columns}
                        getRowId={(v) => v.visitId}
                        loading={isLoading}
                        columnSizingOptions={columnSizingOptions}
                        getRowClassName={(visit) => visit.isFinishRequested ? "bg-(--colorStatusWarningBackground1)" : ""}
                    />
                </Card>

                <div className="flex items-center justify-between flex-wrap gap-2">
                    <Body1>Показано {filteredVisits.length} из {totalCount}</Body1>
                    <Pagination
                        currentPage={currentPage}
                        totalPages={totalPages}
                        onPageChange={setCurrentPage}
                        pageSize={pageSize}
                        onPageSizeChange={setPageSize}
                        totalCount={totalCount}
                    />
                </div>
                <ApproveVisitDialog
                    open={dialogOpen}
                    visit={selectedVisit}
                    onOpenChange={setDialogOpen}
                    onApprove={handleApprove}
                    onReject={handleReject}
                    approving={approving}
                    rejecting={rejecting}
                />
                <WalkInVisitDialog
                    open={walkInOpen}
                    onOpenChange={setWalkInOpen}
                    onSuccess={() => void refetch()}
                />
            </div>
        </RequirePermission>
    );
};


