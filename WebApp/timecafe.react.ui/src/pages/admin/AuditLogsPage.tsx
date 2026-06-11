import { NO_DATA } from "@shared/const/placeholders";
import { useMemo, useState } from "react";
import {
    Body1,
    Body2,
    Button,
    Card,
    Field,
    Input,
    Title2,
    createTableColumn,
    TableCellLayout,
    Dialog,
    DialogSurface,
    DialogBody,
    DialogTitle,
    DialogContent,
    DialogActions,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { Eye20Regular } from "@fluentui/react-icons";
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
import { useGetProfileByUserIdQuery } from "@store/api/profileApi";
import { formatDateTime } from "@utility/dateUtils";
import { DismissableError } from "@components/DismissableError/DismissableError";
import { getUserFullName } from "@utility/userUtils";
import { AuditLogDetails } from "@components/Admin/AuditLogDetails";

const isUuid = (str: string) =>
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(str);

const AuditUserCell = ({ userName }: { userName: string }) => {
    const { data: profile } = useGetProfileByUserIdQuery(userName, {
        skip: !isUuid(userName),
    });

    if (!isUuid(userName)) {
        return <span>{userName}</span>;
    }

    if (!profile) {
        return <span className="text-(--colorNeutralForeground3)">{userName.slice(0, 8)}...</span>;
    }

    const displayName = getUserFullName(profile, userName);
    return <span>{displayName}</span>;
};

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

    const [selectedLog, setSelectedLog] = useState<AuditLogDto | null>(null);
    const [detailsOpen, setDetailsOpen] = useState(false);

    const handleOpenDetails = (log: AuditLogDto) => {
        setSelectedLog(log);
        setDetailsOpen(true);
    };

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        eventType: { minWidth: 120, defaultWidth: 180, idealWidth: 220 },
        action: { minWidth: 120, defaultWidth: 180, idealWidth: 220 },
        userName: { minWidth: 120, defaultWidth: 160, idealWidth: 200 },
        machineName: { minWidth: 120, defaultWidth: 160, idealWidth: 200 },
        duration: { minWidth: 80, defaultWidth: 120, idealWidth: 140 },
        createdAt: { minWidth: 130, defaultWidth: 170, idealWidth: 200 },
        actions: { minWidth: 80, defaultWidth: 100, idealWidth: 120 },
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
            renderCell: (log) => <TableCellLayout truncate><AuditUserCell userName={log.userName} /></TableCellLayout>,
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
        createTableColumn<AuditLogDto>({
            columnId: "actions",
            compare: () => 0,
            renderHeaderCell: () => "Действия",
            renderCell: (log) => (
                <Button
                    appearance="subtle"
                    icon={<Eye20Regular />}
                    onClick={() => handleOpenDetails(log)}
                >
                    Детали
                </Button>
            ),
        }),
    ], []);

    const handleClearFilters = () => {
        setFilters({ eventType: "", userName: "" });
        setCurrentPage(1);
    };

    return (
        <RequirePermission can={Permissions.AuditLogAdminRead}>
            <div className="flex flex-col gap-2">
                <div className="flex items-center justify-between flex-wrap gap-4">
                    <div className="flex flex-col">
                        <Title2>Аудит-логи</Title2>
                        <Body2>{totalCount} записей</Body2>
                    </div>
                </div>

                <div className="flex gap-4 flex-wrap items-end">
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

                <DismissableError error={errorMessage} />

                <Card size={sizes.card}>
                    <DataTable
                        items={logs}
                        columns={columns}
                        getRowId={(log) => log.id}
                        loading={isLoading}
                        columnSizingOptions={columnSizingOptions}
                    />
                </Card>

                <div className="flex items-center justify-between flex-wrap gap-2">
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

            <Dialog open={detailsOpen} onOpenChange={(_, data) => setDetailsOpen(data.open)}>
                <DialogSurface style={{ maxWidth: "800px", width: "100%" }}>
                    <DialogBody>
                        <DialogTitle>Детали записи аудита</DialogTitle>
                        <DialogContent>
                            {selectedLog && (
                                <AuditLogDetails log={selectedLog} userDisplayName={<AuditUserCell userName={selectedLog.userName} />} />
                            )}
                        </DialogContent>
                        <DialogActions>
                            <Button appearance="secondary" onClick={() => setDetailsOpen(false)}>
                                Закрыть
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>
        </RequirePermission>
    );
};
