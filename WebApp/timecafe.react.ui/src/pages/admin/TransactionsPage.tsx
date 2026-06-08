import { useMemo } from "react";
import {
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    Field,
    Input,
    Title2,
    Title3,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type { TableColumnDefinition, TableColumnSizingOptions } from "@fluentui/react-components";
import { useGetAdminTransactionsQuery } from "@store/api/adminApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { DataTable } from "@components/DataTable/DataTable";
import { Pagination } from "@components/Pagination/Pagination";
import { useComponentSize } from "@hooks/useComponentSize";
import { usePermissions } from "@hooks/usePermissions";
import { HasPermission } from "@components/Guard/HasPermission";
import { Permissions, type Permission } from "@shared/auth/permissions";
import type { BillingTransaction } from "@app-types/billing";
import { TransactionType, TransactionStatus } from "@app-types/billing";
import { DismissableError } from "@components/DismissableError/DismissableError";
import { NO_DATA } from "@shared/const/placeholders";
import { formatDateTime } from "@utility/dateUtils";
import { formatMoney } from "@utility/formatUtils";
import { txTypeLabel, txTypeColor, txSourceLabel, txStatusLabel, txStatusColor } from "@utility/billingUtils";
import { usePagination } from "@hooks/usePagination";
import { PageLoader } from "@components/PageLoader/PageLoader";
import { RequirePermission } from "@app/components/RequirePermission/RequirePermission";
import { ArrowClockwise20Regular } from "@fluentui/react-icons";

export const TransactionsPage = () => {
    const { sizes } = useComponentSize();
    const { has } = usePermissions();
    const { page: currentPage, size: pageSize, filters, setPage: setCurrentPage, setSize: setPageSize, setFilters } = usePagination("adminTransactions");
    const userId = filters.userId || "";

    const setUserId = (id: string) => setFilters({ userId: id });

    const { data, isLoading, error, refetch } = useGetAdminTransactionsQuery(
        { page: currentPage, pageSize, userId: userId || undefined },
        { refetchOnMountOrArgChange: true }
    );

    const transactions = useMemo(() => data?.items ?? [], [data]);
    const totalPages = data?.metadata?.totalPages ?? 1;
    const totalCount = data?.metadata?.totalCount ?? 0;
    const errorMessage = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const totalDeposits = useMemo(() => transactions.reduce((sum, item) => sum + (item.type === TransactionType.Deposit ? item.amount : 0), 0), [transactions]);
    const totalWithdrawals = useMemo(() => transactions.reduce((sum, item) => sum + (item.type === TransactionType.Withdrawal ? Math.abs(item.amount) : 0), 0), [transactions]);
    const completedCount = useMemo(() => transactions.filter((item) => item.status === TransactionStatus.Completed).length, [transactions]);

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        date: { minWidth: 130, defaultWidth: 160, idealWidth: 180 },
        user: { minWidth: 150, defaultWidth: 220, idealWidth: 250 },
        type: { minWidth: 100, defaultWidth: 140, idealWidth: 160 },
        source: { minWidth: 100, defaultWidth: 130, idealWidth: 150 },
        status: { minWidth: 100, defaultWidth: 130, idealWidth: 150 },
        amount: { minWidth: 80, defaultWidth: 120, idealWidth: 140 },
        balance: { minWidth: 80, defaultWidth: 120, idealWidth: 140 },
        comment: { minWidth: 100, defaultWidth: 220, idealWidth: 280 },
    }), []);

    const columns: TableColumnDefinition<BillingTransaction>[] = useMemo(() => {
        const allColumns: (TableColumnDefinition<BillingTransaction> & { permission?: string })[] = [
            createTableColumn<BillingTransaction>({
                columnId: "date",
                compare: (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
                renderHeaderCell: () => "Дата",
                renderCell: (tx) => <TableCellLayout truncate>{formatDateTime(tx.createdAt)}</TableCellLayout>,
            }),
            {
                ...createTableColumn<BillingTransaction>({
                    columnId: "user",
                    compare: (a, b) => a.userId.localeCompare(b.userId),
                    renderHeaderCell: () => "Пользователь",
                    renderCell: (tx) => (
                        <TableCellLayout truncate>
                            <div className="min-w-0">
                                <Body2 className="font-mono text-(--colorNeutralForeground4)">{tx.userId.slice(0, 12)}…</Body2>
                            </div>
                        </TableCellLayout>
                    ),
                }),
                permission: Permissions.UserProfileProfileRead
            },
            createTableColumn<BillingTransaction>({
                columnId: "type",
                compare: (a, b) => a.type - b.type,
                renderHeaderCell: () => "Тип",
                renderCell: (tx) => (
                    <TableCellLayout truncate>
                        <Badge appearance="tint" color={txTypeColor(tx.type)}>{txTypeLabel(tx.type)}</Badge>
                    </TableCellLayout>
                ),
            }),
            createTableColumn<BillingTransaction>({
                columnId: "source",
                compare: (a, b) => a.source - b.source,
                renderHeaderCell: () => "Источник",
                renderCell: (tx) => <TableCellLayout truncate>{txSourceLabel(tx.source)}</TableCellLayout>,
            }),
            createTableColumn<BillingTransaction>({
                columnId: "status",
                compare: (a, b) => a.status - b.status,
                renderHeaderCell: () => "Статус",
                renderCell: (tx) => (
                    <TableCellLayout truncate>
                        <Badge appearance="tint" color={txStatusColor(tx.status)}>{txStatusLabel(tx.status)}</Badge>
                    </TableCellLayout>
                ),
            }),
            {
                ...createTableColumn<BillingTransaction>({
                    columnId: "amount",
                    compare: (a, b) => a.amount - b.amount,
                    renderHeaderCell: () => "Сумма",
                    renderCell: (tx) => (
                        <TableCellLayout truncate>
                            <span className={tx.type === TransactionType.Withdrawal ? "text-(--colorPaletteRedForeground1)" : "text-(--colorPaletteGreenForeground1)"}>
                                {tx.type === TransactionType.Withdrawal ? "−" : "+"}{formatMoney(Math.abs(tx.amount))}
                            </span>
                        </TableCellLayout>
                    ),
                }),
                permission: Permissions.BillingTransactionRead
            },
            {
                ...createTableColumn<BillingTransaction>({
                    columnId: "balance",
                    compare: (a, b) => a.balanceAfter - b.balanceAfter,
                    renderHeaderCell: () => "Баланс после",
                    renderCell: (tx) => (
                        <TableCellLayout truncate>
                            {formatMoney(tx.balanceAfter)}
                        </TableCellLayout>
                    ),
                }),
                permission: Permissions.BillingTransactionRead
            },
            createTableColumn<BillingTransaction>({
                columnId: "comment",
                compare: (a, b) => (a.comment ?? "").localeCompare(b.comment ?? ""),
                renderHeaderCell: () => "Комментарий",
                renderCell: (tx) => <TableCellLayout truncate>{tx.comment || NO_DATA}</TableCellLayout>,
            }),
        ];

        return allColumns.filter(col => !col.permission || has(col.permission as Permission));
    }, [has]);


    if (isLoading) return <PageLoader label="Загрузка транзакций..." />;

    return (
        <RequirePermission can={Permissions.BillingBalanceRead}>
        <div className="flex flex-col gap-2">
            <div className="flex items-center justify-between flex-wrap gap-4">
                <div className="flex flex-col">
                    <Title2>Транзакции</Title2>
                    <Body2>Все финансовые операции · {totalCount} записей</Body2>
                </div>
                <Button appearance="subtle" size={sizes.button} icon={<ArrowClockwise20Regular />} onClick={() => refetch()} />
            </div>

            <div className="grid gap-4 md:grid-cols-4 ">
                <Card size={sizes.card}>
                    <Body2>Всего</Body2>
                    <Title3>{totalCount}</Title3>
                </Card>
                <HasPermission can={Permissions.BillingTransactionRead}>
                    <Card size={sizes.card}>
                        <Body2>Пополнения (на стр.)</Body2>
                        <Title3 style={{ color: "var(--colorPaletteGreenForeground1)" }}>{formatMoney(totalDeposits)}</Title3>
                    </Card>
                </HasPermission>
                <HasPermission can={Permissions.BillingTransactionRead}>
                    <Card size={sizes.card}>
                        <Body2>Списания (на стр.)</Body2>
                        <Title3 style={{ color: "var(--colorPaletteRedForeground1)" }}>{formatMoney(totalWithdrawals)}</Title3>
                    </Card>
                </HasPermission>
                <Card size={sizes.card}>
                    <Body2>Выполнено (на стр.)</Body2>
                    <Title3>{completedCount}</Title3>
                </Card>
            </div>

            <div className="flex gap-4 flex-wrap items-end ">
                <Field label="Фильтр по ID пользователя" size={sizes.field}>
                    <Input
                        size={sizes.input}
                        value={userId}
                        onChange={(e) => { setUserId(e.target.value); setCurrentPage(1); }}
                        placeholder="Опционально..."
                        style={{ minWidth: 320 }}
                    />
                </Field>
                {userId && (
                    <Button appearance="secondary" size={sizes.button} onClick={() => { setUserId(""); setCurrentPage(1); }}>
                        Сбросить
                    </Button>
                )}
            </div>

            <DismissableError error={errorMessage} />

            <Card size={sizes.card}>
                <DataTable
                    items={transactions}
                    columns={columns}
                    getRowId={(tx) => tx.transactionId}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between  flex-wrap gap-2">
                <Body1>Показано {transactions.length} из {totalCount}</Body1>
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
