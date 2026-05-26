import { useMemo } from "react";
import { useNavigate } from "react-router-dom";
import {
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    MessageBar,
    MessageBarBody,
    Title2,
    createTableColumn,
    TableCellLayout,
    Spinner,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { Eye20Regular, Clock20Regular } from "@fluentui/react-icons";
import { useGetVisitsPageQuery } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import type { VisitWithTariff } from "@app-types/visitWithTariff";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { VisitStatusBadge } from "@components/VisitStatusBadge";
import { useComponentSize } from "@hooks/useComponentSize";
import { usePermissions } from "@hooks/usePermissions";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions } from "@shared/auth/permissions";

import { CURRENCY_SYMBOL } from "@shared/const/currency";
import { PageLoader } from "@components/PageLoader/PageLoader";
import { NO_DATA } from "@shared/const/placeholders";



const formatDateTime = (iso: string | null) => {
    if (!iso) return NO_DATA;
    const d = new Date(iso);
    return d.toLocaleString("ru-RU", { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit" });
};

const formatCost = (cost: number | null) => {
    if (cost == null) return NO_DATA;
    return `${cost.toFixed(2)} ${CURRENCY_SYMBOL}`;
};

import { usePagination } from "@hooks/usePagination";

export const VisitsPage = () => {
    const navigate = useNavigate();
    const { sizes } = useComponentSize();
    const { has } = usePermissions();
    const { page: currentPage, size: pageSize, setPage: setCurrentPage, setSize: setPageSize } = usePagination("adminVisits");
    const { data, isLoading, error } = useGetVisitsPageQuery(
        { page: currentPage, pageSize },
        { refetchOnMountOrArgChange: true }
    );
    const visits = data?.items ?? [];
    const totalCount = data?.metadata?.totalCount ?? 0;
    const totalPages = data?.metadata?.totalPages ?? 1;
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        tariff: { minWidth: 120, defaultWidth: 200 },
        status: { minWidth: 80, defaultWidth: 120 },
        entryTime: { minWidth: 130, defaultWidth: 170 },
        exitTime: { minWidth: 130, defaultWidth: 170 },
        cost: { minWidth: 80, defaultWidth: 120 },
        userId: { minWidth: 100, defaultWidth: 180 },
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
                        <VisitStatusBadge status={visit.status} />
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
                    <TableCellLayout truncate>{formatDateTime(visit.exitTime)}</TableCellLayout>
                ),
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "cost",
                compare: (a, b) => (a.calculatedCost ?? 0) - (b.calculatedCost ?? 0),
                renderHeaderCell: () => "Стоимость",
                renderCell: (visit) => (
                    <TableCellLayout truncate>{formatCost(visit.calculatedCost)}</TableCellLayout>
                ),
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "userId",
                compare: (a, b) => a.userId.localeCompare(b.userId),
                renderHeaderCell: () => "Пользователь",
                renderCell: (visit) => (
                    <TableCellLayout truncate>
                        <Body2 className="font-mono text-xs">{visit.userId.slice(0, 8)}…</Body2>
                    </TableCellLayout>
                ),
            }),
            createTableColumn<VisitWithTariff>({
                columnId: "actions",
                compare: () => 0,
                renderHeaderCell: () => "Действия",
                renderCell: (visit) => (
                    <HasPermission can={Permissions.VenueVisitRead}>
                        <Button appearance="subtle" icon={<Eye20Regular />} onClick={() => navigate(`/admin/visits/${visit.visitId}`)}>
                            Открыть
                        </Button>
                    </HasPermission>
                ),
            }),
        ];

        return allColumns.filter(col => !col.permission || has(col.permission as any));
    }, [navigate, has]);

    if (isLoading) {
        return <PageLoader label="Загрузка визитов..." />;
    }

    if (queryError) {
        return (
            <MessageBar intent="error" className="mb-4">
                <MessageBarBody>{queryError}</MessageBarBody>
            </MessageBar>
        );
    }

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Визиты</Title2>
                    <Body2 block>{totalCount} визитов</Body2>
                </div>
                <HasPermission can={Permissions.VenueVisitViewPending}>
                    <Button
                        appearance="outline"
                        icon={<Clock20Regular />}
                        onClick={() => navigate("/admin/visits/pending")}
                    >
                        Ожидают подтверждения
                    </Button>
                </HasPermission>
            </div>

            <Card className="overflow-x-auto" size={sizes.card}>
                <DataTable
                    items={visits}
                    columns={columns}
                    getRowId={(v) => v.visitId}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
                <Body1>Показано {visits.length} из {totalCount}</Body1>
                <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    onPageChange={setCurrentPage}
                    pageSize={pageSize}
                    onPageSizeChange={setPageSize}
                    totalCount={totalCount}
                />
            </div>
        </div>
    );
};
