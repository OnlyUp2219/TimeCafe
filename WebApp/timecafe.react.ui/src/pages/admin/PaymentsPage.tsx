import { NO_DATA } from "@shared/const/placeholders";
import { useMemo } from "react";
import {
    Badge,
    Body1,
    Body2,
    Card,
    Caption1,
    Field,
    Input,
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { useGetAdminPaymentsQuery } from "@store/api/adminApi";
import type { AdminPaymentDto } from "@store/api/adminApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { useComponentSize } from "@hooks/useComponentSize";
import { usePermissions } from "@hooks/usePermissions";
import { Permissions, type Permission } from "@shared/auth/permissions";
import { usePagination } from "@hooks/usePagination";
import { DismissableError } from "@components/DismissableError/DismissableError";

import { paymentStatusLabel, paymentStatusColor, paymentMethodLabel } from "@utility/billingUtils";
import { formatDateTime as formatDate } from "@utility/dateUtils";
import { formatMoney } from "@utility/formatUtils";
import { UserCell } from "@components/DataTable/cells/UserCell";

export const PaymentsPage = () => {
    const { sizes } = useComponentSize();
    const { has } = usePermissions();
    const { page: currentPage, size: pageSize, filters, setPage: setCurrentPage, setSize: setPageSize, setFilters } = usePagination("adminPayments");
    const userIdFilter = filters.userIdFilter || "";

    const setUserIdFilter = (id: string) => setFilters({ userIdFilter: id });

    const { data, isLoading, error } = useGetAdminPaymentsQuery(
        { page: currentPage, pageSize, userId: userIdFilter || undefined },
        { refetchOnMountOrArgChange: true }
    );

    const payments = data?.items ?? [];
    const totalCount = data?.metadata?.totalCount ?? 0;
    const totalPages = data?.metadata?.totalPages ?? 1;
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        userId: { minWidth: 100, defaultWidth: 160, idealWidth: 300 },
        amount: { minWidth: 80, defaultWidth: 120, idealWidth: 140 },
        method: { minWidth: 80, defaultWidth: 110, idealWidth: 130 },
        status: { minWidth: 100, defaultWidth: 130, idealWidth: 150 },
        externalId: { minWidth: 100, defaultWidth: 180, idealWidth: 220 },
        createdAt: { minWidth: 130, defaultWidth: 160, idealWidth: 180 },
    }), []);

    const columns: TableColumnDefinition<AdminPaymentDto>[] = useMemo(() => {
        const allColumns: (TableColumnDefinition<AdminPaymentDto> & { permission?: string })[] = [
            createTableColumn<AdminPaymentDto>({
                columnId: "userId",
                compare: (a, b) => a.userId.localeCompare(b.userId),
                renderHeaderCell: () => "Пользователь",
                renderCell: (p) => <UserCell userId={p.userId} variant="detailed" showAvatar readOnly />,
            }),
            {
                ...createTableColumn<AdminPaymentDto>({
                    columnId: "amount",
                    compare: (a, b) => a.amount - b.amount,
                    renderHeaderCell: () => "Сумма",
                    renderCell: (p) => (
                        <TableCellLayout truncate>
                            <Body1 className="text-(--colorPaletteGreenForeground1)">{formatMoney(p.amount)}</Body1>
                        </TableCellLayout>
                    ),
                }),
                permission: Permissions.BillingPaymentHistoryRead
            },
            createTableColumn<AdminPaymentDto>({
                columnId: "method",
                compare: (a, b) => a.paymentMethod - b.paymentMethod,
                renderHeaderCell: () => "Метод",
                renderCell: (p) => <TableCellLayout truncate>{paymentMethodLabel(p.paymentMethod)}</TableCellLayout>,
            }),
            createTableColumn<AdminPaymentDto>({
                columnId: "status",
                compare: (a, b) => a.status - b.status,
                renderHeaderCell: () => "Статус",
                renderCell: (p) => (
                    <TableCellLayout truncate>
                        <Badge appearance="tint" color={paymentStatusColor(p.status)}>{paymentStatusLabel(p.status)}</Badge>
                    </TableCellLayout>
                ),
            }),
            createTableColumn<AdminPaymentDto>({
                columnId: "externalId",
                compare: (a, b) => (a.externalPaymentId ?? "").localeCompare(b.externalPaymentId ?? ""),
                renderHeaderCell: () => "External ID",
                renderCell: (p) => (
                    <TableCellLayout truncate>
                        <Caption1 className="font-mono">{p.externalPaymentId ? p.externalPaymentId.slice(0, 16) + "…" : NO_DATA}</Caption1>
                    </TableCellLayout>
                ),
            }),
            createTableColumn<AdminPaymentDto>({
                columnId: "createdAt",
                compare: (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
                renderHeaderCell: () => "Создан",
                renderCell: (p) => <TableCellLayout truncate>{formatDate(p.createdAt)}</TableCellLayout>,
            }),
        ];

        return allColumns.filter(col => !col.permission || has(col.permission as Permission));
    }, [has]);

    return (
        <div className="flex flex-col gap-2">
            <div className="flex items-center justify-between flex-wrap gap-4">
                <div className="flex flex-col">
                    <Title2>Платежи</Title2>
                    <Body2>{totalCount} платежей</Body2>
                </div>
            </div>

            <div className="flex gap-4 flex-wrap items-end">
                <Field label="Фильтр по userId" size={sizes.field}>
                    <Input
                        size={sizes.input}
                        value={userIdFilter}
                        onChange={(e) => { setUserIdFilter(e.target.value); setCurrentPage(1); }}
                        placeholder="Введите userId..."
                        style={{ minWidth: 280 }}
                    />
                </Field>
            </div>

            <DismissableError error={queryError} className="mb-4" />

            <Card size={sizes.card}>
                <DataTable
                    items={payments}
                    columns={columns}
                    getRowId={(p) => p.paymentId}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
                <Body1>Показано {payments.length} из {totalCount}</Body1>
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


