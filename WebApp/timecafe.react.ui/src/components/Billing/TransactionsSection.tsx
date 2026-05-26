import { useMemo } from "react";
import {
    Card,
    Title2,
    type TableColumnDefinition,
    createTableColumn,
    Badge,
    TableCellLayout,
    type TableColumnSizingOptions,
    Body1
} from "@fluentui/react-components";

import {
    TransactionSource,
    TransactionStatus,
    TransactionType,
    type BillingTransaction,
} from "@app-types/billing";

import { DataTable } from "@components/DataTable";
import { CURRENCY_SYMBOL } from "@shared/const/currency";
import { useComponentSize } from "@hooks/useComponentSize";
import { Pagination } from "@components/Pagination";

type TransactionsSectionProps = {
    transactions: BillingTransaction[];
    loading: boolean;
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
};

const formatDateTime = (iso: string) =>
    new Date(iso).toLocaleString("ru-RU", { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit" });

const formatMoney = (v: number) => `${v.toFixed(2)} ${CURRENCY_SYMBOL}`;

const txTypeLabel = (t: number) => {
    switch (t) {
        case TransactionType.Deposit: return "Пополнение";
        case TransactionType.Withdrawal: return "Списание";
        case TransactionType.Adjustment: return "Корректировка";
        default: return "Нет данных";
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
        default: return "Нет данных";
    }
};

const txStatusLabel = (s: number) => {
    switch (s) {
        case TransactionStatus.Pending: return "Ожидание";
        case TransactionStatus.Completed: return "Выполнена";
        case TransactionStatus.Failed: return "Ошибка";
        case TransactionStatus.PartialCompleted: return "Частично";
        default: return "Нет данных";
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

export const TransactionsSection = ({
    transactions,
    loading,
    currentPage,
    totalPages,
    onPageChange,
}: TransactionsSectionProps) => {
    const { sizes } = useComponentSize();

    const columnSizingOptions: TableColumnSizingOptions = useMemo(() => ({
        date: { minWidth: 130, defaultWidth: 160, idealWidth: 180 },
        type: { minWidth: 100, defaultWidth: 140, idealWidth: 160 },
        source: { minWidth: 100, defaultWidth: 130, idealWidth: 150 },
        status: { minWidth: 100, defaultWidth: 130, idealWidth: 150 },
        amount: { minWidth: 80, defaultWidth: 120, idealWidth: 140 },
        balance: { minWidth: 80, defaultWidth: 120, idealWidth: 140 },
        comment: { minWidth: 100, defaultWidth: 220, idealWidth: 280 },
    }), []);

    const columns: TableColumnDefinition<BillingTransaction>[] = useMemo(() => [
        createTableColumn<BillingTransaction>({
            columnId: "date",
            compare: (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
            renderHeaderCell: () => "Дата",
            renderCell: (tx) => <TableCellLayout truncate>{formatDateTime(tx.createdAt)}</TableCellLayout>,
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
                    <span className={tx.type === TransactionType.Withdrawal ? "text-[var(--colorPaletteRedForeground1)]" : "text-[var(--colorPaletteGreenForeground1)]"}>
                        {tx.type === TransactionType.Withdrawal ? "−" : "+"}{formatMoney(Math.abs(tx.amount))}
                    </span>
                </TableCellLayout>
            ),
        }),
        createTableColumn<BillingTransaction>({
            columnId: "comment",
            compare: (a, b) => (a.comment ?? "").localeCompare(b.comment ?? ""),
            renderHeaderCell: () => "Комментарий",
            renderCell: (tx) => <TableCellLayout truncate>{tx.comment || "—"}</TableCellLayout>,
        }),
    ], []);

    return (
        <Card className="flex flex-col gap-4 h-full" size={sizes.card}>
            <div className="flex items-center justify-between">
                <Title2>История операций</Title2>
            </div>

            <div className="overflow-hidden">
                <DataTable
                    items={transactions}
                    columns={columns}
                    getRowId={(tx) => tx.transactionId}
                    loading={loading}
                    emptyMessage="Транзакции пока отсутствуют"
                    columnSizingOptions={columnSizingOptions}
                />
            </div>

            <div className="flex items-center justify-between flex-wrap gap-2">
                <Body1>Показано {transactions.length}</Body1>
                <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    onPageChange={onPageChange}
                />
            </div>
        </Card>
    );
};
