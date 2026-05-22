import { useMemo } from "react";
import {
    Body1,
    Body2,
    Button,
    Card,
    Field,
    Input,
    MessageBar,
    MessageBarBody,
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { RequirePermission } from "@components/RequirePermission/RequirePermission";
import { Permissions } from "@shared/auth/permissions";
import { useGetAuditLogsQuery } from "@store/api/adminApi";
import type { AuditLogDto } from "@store/api/adminApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { useComponentSize } from "@hooks/useComponentSize";
import { usePagination } from "@hooks/usePagination";


const formatDateTime = (iso: string) =>
    new Date(iso).toLocaleString("ru-RU", { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit" });

const formatDuration = (ms: number) => {
    if (ms < 1000) return `${ms} мс`;
    if (ms < 60000) return `${(ms / 1000).toFixed(2)} с`;
    const minutes = Math.floor(ms / 60000);
    const seconds = ((ms % 60000) / 1000).toFixed(0);
    return `${minutes} мин ${seconds} с`;
};

export const AuditLogsPage = () => {
    const { sizes } = useComponentSize();
    const { page: currentPage, size: pageSize, filters, setPage: setCurrentPage, setSize: setPageSize, setFilters } = usePagination("adminAuditLogs");

    const eventTypeFilter = filters.eventType || "";
    const userNameFilter = filters.userName || "";

    const setEventTypeFilter = (value: string) => setFilters({ eventType: value });
    const setUserNameFilter = (value: string) => setFilters({ userName: value });

    const { data, isLoading, error } = useGetAuditLogsQuery({
        page: currentPage,
        pageSize,
        eventType: eventTypeFilter || undefined,
        userName: userNameFilter || undefined,
    });

    const logs = data?.items ?? [];
    const totalPages = data?.metadata.totalPages ?? 1;
    const totalCount = data?.metadata.totalCount ?? 0;
    const errorMessage = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        eventType: { minWidth: 120, defaultWidth: 180, idealWidth: 220 },
        action: { minWidth: 120, defaultWidth: 180, idealWidth: 220 },
        userName: { minWidth: 120, defaultWidth: 160, idealWidth: 200 },
        machineName: { minWidth: 120, defaultWidth: 160, idealWidth: 200 },
        duration: { minWidth: 80, defaultWidth: 120, idealWidth: 140 },
        createdAt: { minWidth: 130, defaultWidth: 170, idealWidth: 200 },
    }), []);

    const columns: TableColumnDefinition<AuditLogDto>[] = useMemo(() => [
        createTableColumn<AuditLogDto>({
            columnId: "eventType",
            compare: (a, b) => a.eventType.localeCompare(b.eventType),
            renderHeaderCell: () => "Тип события",
            renderCell: (log) => <TableCellLayout truncate>{log.eventType}</TableCellLayout>,
        }),
        createTableColumn<AuditLogDto>({
            columnId: "action",
            compare: (a, b) => a.action.localeCompare(b.action),
            renderHeaderCell: () => "Действие",
            renderCell: (log) => <TableCellLayout truncate>{log.action}</TableCellLayout>,
        }),
        createTableColumn<AuditLogDto>({
            columnId: "userName",
            compare: (a, b) => a.userName.localeCompare(b.userName),
            renderHeaderCell: () => "Пользователь",
            renderCell: (log) => <TableCellLayout truncate>{log.userName}</TableCellLayout>,
        }),
        createTableColumn<AuditLogDto>({
            columnId: "machineName",
            compare: (a, b) => a.machineName.localeCompare(b.machineName),
            renderHeaderCell: () => "Машина",
            renderCell: (log) => <TableCellLayout truncate>{log.machineName}</TableCellLayout>,
        }),
        createTableColumn<AuditLogDto>({
            columnId: "duration",
            compare: (a, b) => a.duration - b.duration,
            renderHeaderCell: () => "Длительность",
            renderCell: (log) => <TableCellLayout truncate>{formatDuration(log.duration)}</TableCellLayout>,
        }),
        createTableColumn<AuditLogDto>({
            columnId: "createdAt",
            compare: (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
            renderHeaderCell: () => "Дата",
            renderCell: (log) => <TableCellLayout truncate>{formatDateTime(log.createdAt)}</TableCellLayout>,
        }),
    ], []);

    const handleClearFilters = () => {
        setFilters({ eventType: "", userName: "" });
        setCurrentPage(1);
    };

    return (
        <RequirePermission permission={Permissions.AuditLogAdminRead} fallback={
            <MessageBar intent="error" className="mb-4">
                <MessageBarBody>Недостаточно прав для просмотра аудит-логов.</MessageBarBody>
            </MessageBar>
        }>
            <div>
                <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                    <div>
                        <Title2>Аудит-логи</Title2>
                        <Body2 block>{totalCount} записей</Body2>
                    </div>
                </div>

                <div className="flex gap-4 flex-wrap items-end mb-4">
                    <Field label="Тип события" size={sizes.field}>
                        <Input
                            size={sizes.input}
                            value={eventTypeFilter}
                            onChange={(e) => { setEventTypeFilter(e.target.value); setCurrentPage(1); }}
                            placeholder="Фильтр по типу..."
                        />
                    </Field>
                    <Field label="Пользователь" size={sizes.field}>
                        <Input
                            size={sizes.input}
                            value={userNameFilter}
                            onChange={(e) => { setUserNameFilter(e.target.value); setCurrentPage(1); }}
                            placeholder="Фильтр по пользователю..."
                        />
                    </Field>
                    {(eventTypeFilter || userNameFilter) && (
                        <Button appearance="secondary" size={sizes.button} onClick={handleClearFilters}>
                            Сбросить фильтры
                        </Button>
                    )}
                </div>

                {errorMessage && (
                    <MessageBar intent="error" className="mb-4">
                        <MessageBarBody>{errorMessage}</MessageBarBody>
                    </MessageBar>
                )}

                <Card size={sizes.card}>
                    <DataTable
                        items={logs}
                        columns={columns}
                        getRowId={(log) => log.id}
                        loading={isLoading}
                        columnSizingOptions={columnSizingOptions}
                    />
                </Card>

                <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
                    <Body1>Показано {logs.length} из {totalCount}</Body1>
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
        </RequirePermission>
    );
};
