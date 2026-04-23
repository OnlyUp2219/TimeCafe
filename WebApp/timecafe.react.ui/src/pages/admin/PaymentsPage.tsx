import {useMemo, useState} from "react";
import {
    Avatar,
    Badge,
    Body1,
    Body2,
    Card,
    Caption1,
    Field,
    Input,
    MessageBar,
    MessageBarBody,
    Title2,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {useGetAdminPaymentsQuery} from "@store/api/adminApi";
import type {AdminPaymentDto} from "@store/api/adminApi";
import {useGetProfileByUserIdQuery} from "@store/api/profileApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {DataTable} from "@components/DataTable/DataTable";
import {Pagination} from "@components/Pagination/Pagination";
import {useComponentSize} from "@hooks/useComponentSize";
import {usePermissions} from "@hooks/usePermissions";
import {HasPermission} from "@components/Guard/HasPermission";
import {Permissions} from "@shared/auth/permissions";

import {NO_ACCESS} from "@shared/const/placeholders";
import {CURRENCY_SYMBOL} from "@shared/const/currency";

const paymentStatusLabel = (s: number) => {
    switch (s) {
        case 0: return "Ожидание";
        case 1: return "Выполнен";
        case 2: return "Ошибка";
        case 3: return "Возврат";
        case 4: return "Отменён";
        default: return "—";
    }
};

const paymentStatusColor = (s: number): "warning" | "success" | "danger" | "informative" => {
    switch (s) {
        case 0: return "warning";
        case 1: return "success";
        case 2: return "danger";
        case 3: return "informative";
        case 4: return "danger";
        default: return "warning";
    }
};

const paymentMethodLabel = (m: number) => {
    switch (m) {
        case 0: return "Stripe";
        case 1: return "Вручную";
        default: return "—";
    }
};

const formatMoney = (v: number) => `${v.toFixed(2)} ${CURRENCY_SYMBOL}`;
const formatDate = (iso: string) =>
    new Date(iso).toLocaleString("ru-RU", {day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit"});

const AdminUserCell = ({userId}: {userId: string}) => {
    const {data: profile} = useGetProfileByUserIdQuery(userId);
    const displayName = profile?.firstName || profile?.lastName
        ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim()
        : profile?.email || null;

    return (
        <TableCellLayout truncate media={<Avatar name={displayName || userId} size={28} />}>
            <div className="flex flex-col min-w-0">
                <Body1 block truncate>{displayName || userId}</Body1>
                <Caption1 block className="font-mono text-gray-400" style={{ fontSize: '10px' }}>{userId}</Caption1>
            </div>
        </TableCellLayout>
    );
};

export const PaymentsPage = () => {
    const {sizes} = useComponentSize();
    const {has} = usePermissions();
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);
    const [userIdFilter, setUserIdFilter] = useState("");

    const {data, isLoading, error} = useGetAdminPaymentsQuery(
        {page: currentPage, pageSize, userId: userIdFilter || undefined},
        {refetchOnMountOrArgChange: true}
    );

    const payments = data?.payments ?? [];
    const totalCount = data?.pagination.totalCount ?? 0;
    const totalPages = data?.pagination.totalPages ?? 1;
    const queryError = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        userId: {minWidth: 100, defaultWidth: 160, idealWidth: 200},
        amount: {minWidth: 80, defaultWidth: 120, idealWidth: 140},
        method: {minWidth: 80, defaultWidth: 110, idealWidth: 130},
        status: {minWidth: 100, defaultWidth: 130, idealWidth: 150},
        externalId: {minWidth: 100, defaultWidth: 180, idealWidth: 220},
        createdAt: {minWidth: 130, defaultWidth: 160, idealWidth: 180},
    }), []);

    const columns: TableColumnDefinition<AdminPaymentDto>[] = useMemo(() => {
        const allColumns: (TableColumnDefinition<AdminPaymentDto> & { permission?: string })[] = [
            createTableColumn<AdminPaymentDto>({
                columnId: "userId",
                compare: (a, b) => a.userId.localeCompare(b.userId),
                renderHeaderCell: () => "Пользователь",
                renderCell: (p) => <AdminUserCell userId={p.userId} />,
            }),
            {
                ...createTableColumn<AdminPaymentDto>({
                    columnId: "amount",
                    compare: (a, b) => a.amount - b.amount,
                    renderHeaderCell: () => "Сумма",
                    renderCell: (p) => (
                        <TableCellLayout truncate>
                            <HasPermission can={Permissions.BillingPaymentRead} fallback={NO_ACCESS}>
                                <Body1 style={{ color: "var(--colorPaletteGreenForeground1)" }}>{formatMoney(p.amount)}</Body1>
                            </HasPermission>
                        </TableCellLayout>
                    ),
                }),
                permission: Permissions.BillingPaymentRead
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
                        <Caption1 className="font-mono">{p.externalPaymentId ? p.externalPaymentId.slice(0, 16) + "…" : "—"}</Caption1>
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

        return allColumns.filter(col => !col.permission || has(col.permission as any));
    }, [has]);

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Платежи</Title2>
                    <Body2 block>{totalCount} платежей</Body2>
                </div>
            </div>

            <div className="flex gap-4 flex-wrap items-end mb-4">
                <Field label="Фильтр по userId" size={sizes.field}>
                    <Input
                        size={sizes.input}
                        value={userIdFilter}
                        onChange={(e) => { setUserIdFilter(e.target.value); setCurrentPage(1); }}
                        placeholder="Введите userId..."
                        style={{minWidth: 280}}
                    />
                </Field>
            </div>

            {queryError && (
                <MessageBar intent="error" className="mb-4">
                    <MessageBarBody>{queryError}</MessageBarBody>
                </MessageBar>
            )}

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
