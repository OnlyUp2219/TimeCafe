import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
    Body1,
    Body2,
    Button,
    Card,
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import { DismissableError } from "@components/DismissableError/DismissableError";

import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { ArrowLeft20Regular, CheckmarkCircle20Regular } from "@fluentui/react-icons";
import { useGetPendingVisitsQuery, useApproveVisitMutation, useRejectVisitMutation } from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import type { VisitWithTariff } from "@app-types/visitWithTariff";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { useComponentSize } from "@hooks/useComponentSize";
import { VisitStatusBadge } from "@components/VisitStatusBadge";
import { ApproveVisitDialog } from "@components/Admin/ApproveVisitDialog/ApproveVisitDialog";
import { usePagination } from "@hooks/usePagination";
import { NO_DATA } from "@shared/const/placeholders";
import { PageLoader } from "@components/PageLoader/PageLoader";
import { formatDateTime } from "@utility/dateUtils";
import { UserCell } from "@components/DataTable/cells/UserCell";

export const PendingVisitsPage = () => {


    const navigate = useNavigate();
    const { sizes } = useComponentSize();
    const { page: currentPage, size: pageSize, setPage: setCurrentPage, setSize: setPageSize } = usePagination("adminPendingVisits");
    const { data, isLoading, error } = useGetPendingVisitsQuery(
        { page: currentPage, pageSize },
        { refetchOnMountOrArgChange: true, pollingInterval: 5000 }
    );
    const visits = data?.items ?? [];
    const totalCount = data?.metadata?.totalCount ?? 0;
    const totalPages = data?.metadata?.totalPages ?? 1;
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const [selectedVisit, setSelectedVisit] = useState<VisitWithTariff | null>(null);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [actionError, setActionError] = useState<string | null>(null);

    const [approveVisit, { isLoading: approving }] = useApproveVisitMutation();
    const [rejectVisit, { isLoading: rejecting }] = useRejectVisitMutation();

    const handleApprove = async (visitId: string) => {
        setActionError(null);
        try {
            await approveVisit(visitId).unwrap();
            setDialogOpen(false);
        } catch (err) {
            setActionError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось подтвердить визит");
        }
    };

    const handleReject = async (visitId: string, reason: string) => {
        setActionError(null);
        try {
            await rejectVisit({ visitId, reason }).unwrap();
            setDialogOpen(false);
        } catch (err) {
            setActionError(getRtkErrorMessage(err as FetchBaseQueryError) || "Не удалось отклонить визит");
        }
    };

    const openDialog = (visit: VisitWithTariff) => {
        setSelectedVisit(visit);
        setDialogOpen(true);
    };

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        tariff: { minWidth: 120, defaultWidth: 200 },
        status: { minWidth: 140, defaultWidth: 180 },
        entryTime: { minWidth: 130, defaultWidth: 170 },
        userId: { minWidth: 100, defaultWidth: 180 },
        actions: { minWidth: 120, defaultWidth: 200 },
    }), []);

    const columns: TableColumnDefinition<VisitWithTariff>[] = useMemo(() => [
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
            renderHeaderCell: () => "Время запроса",
            renderCell: (visit) => (
                <TableCellLayout truncate>{formatDateTime(visit.entryTime)}</TableCellLayout>
            ),
        }),
        createTableColumn<VisitWithTariff>({
            columnId: "userId",
            compare: (a, b) => (a.userId ?? "").localeCompare(b.userId ?? ""),
            renderHeaderCell: () => "Пользователь",
            renderCell: (visit) => <UserCell userId={visit.userId ?? ""} />,
        }),
        createTableColumn<VisitWithTariff>({
            columnId: "actions",
            compare: () => 0,
            renderHeaderCell: () => "Действия",
            renderCell: (visit) => (
                <div className="flex gap-2">
                    <Button appearance="primary" size="small" icon={<CheckmarkCircle20Regular />} onClick={() => openDialog(visit)}>
                        Подтвердить
                    </Button>
                </div>
            ),
        }),
    ], []);

    if (isLoading) {
        return <PageLoader label="Загрузка ожидающих визитов..." />;
    }

    return (
        <div className="flex flex-col gap-2">
            <div>
                <Button appearance="subtle" icon={<ArrowLeft20Regular />} onClick={() => navigate("/admin/visits")}>
                    Назад к визитам
                </Button>
            </div>

            <div className="flex items-center justify-between flex-wrap gap-4">
                <div className="flex flex-col">
                    <Title2>Ожидают подтверждения</Title2>
                    <Body2>{totalCount} визитов</Body2>
                </div>
            </div>

            <div className="flex flex-col gap-2">
                <DismissableError error={queryError} />
                <DismissableError error={actionError} />
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

            <div className="flex items-center justify-between flex-wrap gap-2">
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

            <ApproveVisitDialog
                open={dialogOpen}
                visit={selectedVisit}
                onOpenChange={setDialogOpen}
                onApprove={handleApprove}
                onReject={handleReject}
                approving={approving}
                rejecting={rejecting}
            />
        </div>
    );
};
