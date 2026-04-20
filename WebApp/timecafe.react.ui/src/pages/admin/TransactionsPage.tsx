import {useMemo, useState} from "react";
import {
    Badge,
    Body1,
    Body2,
    Button,
    Card,
    Field,
    Input,
    MessageBar,
    MessageBarBody,
    Title2,
    Title3,
    createTableColumn,
    TableCellLayout,
} from "@fluentui/react-components";
import type {TableColumnDefinition, TableColumnSizingOptions} from "@fluentui/react-components";
import {useGetAdminTransactionsQuery} from "@store/api/adminApi";
import {getRtkErrorMessage} from "@shared/api/errors/extractRtkError";
import type {FetchBaseQueryError} from "@reduxjs/toolkit/query";
import {DataTable} from "@components/DataTable/DataTable";
import {Pagination} from "@components/Pagination/Pagination";
import {useComponentSize} from "@hooks/useComponentSize";
import type {BillingTransaction} from "@app-types/billing";
import {TransactionType, TransactionSource, TransactionStatus} from "@app-types/billing";

import {CURRENCY_SYMBOL} from "@shared/const/currency";
import {NO_DATA} from "@shared/const/placeholders";

const formatDateTime = (iso: string) =>
    new Date(iso).toLocaleString("ru-RU", {day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit"});

const formatMoney = (v: number) => `${v.toFixed(2)} ${CURRENCY_SYMBOL}`;

const txTypeLabel = (t: number) => {
    switch (t) {
        case TransactionType.Deposit: return "Пополнение";
        case TransactionType.Withdrawal: return "Списание";
        case TransactionType.Adjustment: return "Корректировка";
        default: return NO_DATA;
    }
};

const txTypeColor = (t: number): "success" | "danger" | "informative" => {
    switch (t) {
        case TransactionType.Deposit: return "success";
        case TransactionType.Withdrawal: return "danger";
        default: return "informative";
    }
};

const txSourceLabel = (s: number) => {
    switch (s) {
        case TransactionSource.Visit: return "Визит";
        case TransactionSource.Manual: return "Вручную";
        case TransactionSource.Payment: return "Платёж";
        case TransactionSource.Refund: return "Возврат";
        default: return NO_DATA;
    }
};

const txStatusLabel = (s: number) => {
    switch (s) {
        case TransactionStatus.Pending: return "Ожидание";
        case TransactionStatus.Completed: return "Выполнена";
        case TransactionStatus.Failed: return "Ошибка";
        case TransactionStatus.PartialCompleted: return "Частично";
        default: return NO_DATA;
    }
};

const txStatusColor = (s: number): "warning" | "success" | "danger" | "informative" => {
    switch (s) {
        case TransactionStatus.Pending: return "warning";
        case TransactionStatus.Completed: return "success";
        case TransactionStatus.Failed: return "danger";
        default: return "informative";
    }
};

export const TransactionsPage = () => {
    const {sizes} = useComponentSize();
    const [userId, setUserId] = useState("");
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(20);

    const {data, isLoading, error} = useGetAdminTransactionsQuery(
        {page: currentPage, pageSize, userId: userId || undefined},
    );

    const transactions = data?.transactions ?? [];
    const totalPages = data?.pagination.totalPages ?? 1;
    const totalCount = data?.pagination.totalCount ?? 0;
    const errorMessage = error ? getRtkErrorMessage(error as FetchBaseQueryError) : null;

    const totalDeposits = useMemo(() => transactions.reduce((sum, item) => sum + (item.type === TransactionType.Deposit ? item.amount : 0), 0), [transactions]);
    const totalWithdrawals = useMemo(() => transactions.reduce((sum, item) => sum + (item.type === TransactionType.Withdrawal ? Math.abs(item.amount) : 0), 0), [transactions]);
    const completedCount = useMemo(() => transactions.filter((item) => item.status === TransactionStatus.Completed).length, [transactions]);

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        date: {minWidth: 130, defaultWidth: 160, idealWidth: 180},
        user: {minWidth: 150, defaultWidth: 220, idealWidth: 250},
        type: {minWidth: 100, defaultWidth: 140, idealWidth: 160},
        source: {minWidth: 100, defaultWidth: 130, idealWidth: 150},
        status: {minWidth: 100, defaultWidth: 130, idealWidth: 150},
        amount: {minWidth: 80, defaultWidth: 120, idealWidth: 140},
        balance: {minWidth: 80, defaultWidth: 120, idealWidth: 140},
        comment: {minWidth: 100, defaultWidth: 220, idealWidth: 280},
    }), []);

    const columns: TableColumnDefinition<BillingTransaction>[] = useMemo(() => [
        createTableColumn<BillingTransaction>({
            columnId: "date",
            compare: (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
            renderHeaderCell: () => "Дата",
            renderCell: (tx) => <TableCellLayout truncate>{formatDateTime(tx.createdAt)}</TableCellLayout>,
        }),
        createTableColumn<BillingTransaction>({
            columnId: "user",
            compare: (a, b) => a.userId.localeCompare(b.userId),
            renderHeaderCell: () => "Пользователь",
            renderCell: (tx) => (
                <TableCellLayout truncate>
                    <div className="min-w-0">
                        <Body2 block className="font-mono text-gray-400">{tx.userId.slice(0, 12)}…</Body2>
                    </div>
                </TableCellLayout>
            ),
        }),
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
        createTableColumn<BillingTransaction>({
            columnId: "amount",
            compare: (a, b) => a.amount - b.amount,
            renderHeaderCell: () => "Сумма",
            renderCell: (tx) => (
                <TableCellLayout truncate>
                    <span className={tx.type === TransactionType.Withdrawal ? "text-red-500" : "text-green-600"}>
                        {tx.type === TransactionType.Withdrawal ? "−" : "+"}{formatMoney(Math.abs(tx.amount))}
                    </span>
                </TableCellLayout>
            ),
        }),
        createTableColumn<BillingTransaction>({
            columnId: "balance",
            compare: (a, b) => a.balanceAfter - b.balanceAfter,
            renderHeaderCell: () => "Баланс после",
            renderCell: (tx) => <TableCellLayout truncate>{formatMoney(tx.balanceAfter)}</TableCellLayout>,
        }),
        createTableColumn<BillingTransaction>({
            columnId: "comment",
            compare: (a, b) => (a.comment ?? "").localeCompare(b.comment ?? ""),
            renderHeaderCell: () => "Комментарий",
            renderCell: (tx) => <TableCellLayout truncate>{tx.comment || NO_DATA}</TableCellLayout>,
        }),
    ], []);

    return (
        <div>
            <div className="flex items-center justify-between mb-4 flex-wrap gap-4">
                <div>
                    <Title2>Транзакции</Title2>
                    <Body2 block>Все финансовые операции · {totalCount} записей</Body2>
                </div>
            </div>

            <div className="grid gap-4 md:grid-cols-4 mb-4">
                <Card size={sizes.card}>
                    <Body2 block>Всего</Body2>
                    <Title3>{totalCount}</Title3>
                </Card>
                <Card size={sizes.card}>
                    <Body2 block>Пополнения (на стр.)</Body2>
                    <Title3 style={{color: "var(--colorPaletteGreenForeground1)"}}>{formatMoney(totalDeposits)}</Title3>
                </Card>
                <Card size={sizes.card}>
                    <Body2 block>Списания (на стр.)</Body2>
                    <Title3 style={{color: "var(--colorPaletteRedForeground1)"}}>{formatMoney(totalWithdrawals)}</Title3>
                </Card>
                <Card size={sizes.card}>
                    <Body2 block>Выполнено (на стр.)</Body2>
                    <Title3>{completedCount}</Title3>
                </Card>
            </div>

            <div className="flex gap-4 flex-wrap items-end mb-4">
                <Field label="Фильтр по ID пользователя" size={sizes.field}>
                    <Input
                        size={sizes.input}
                        value={userId}
                        onChange={(e) => { setUserId(e.target.value); setCurrentPage(1); }}
                        placeholder="Опционально..."
                        style={{minWidth: 320}}
                    />
                </Field>
                {userId && (
                    <Button appearance="secondary" size={sizes.button} onClick={() => { setUserId(""); setCurrentPage(1); }}>
                        Сбросить
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
                    items={transactions}
                    columns={columns}
                    getRowId={(tx) => tx.transactionId}
                    loading={isLoading}
                    columnSizingOptions={columnSizingOptions}
                />
            </Card>

            <div className="flex items-center justify-between mt-4 flex-wrap gap-2">
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
    );
};
