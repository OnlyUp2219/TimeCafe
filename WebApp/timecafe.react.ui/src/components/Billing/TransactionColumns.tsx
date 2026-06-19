import { Badge, TableCellLayout } from "@fluentui/react-components";
import { createTableColumn } from "@fluentui/react-components";
import type { TableColumnDefinition } from "@fluentui/react-components";
import { TransactionType, type BillingTransaction } from "@app-types/billing";
import { NO_DATA } from "@shared/const/placeholders";
import { formatDateTime } from "@utility/dateUtils";
import { formatMoney } from "@utility/formatUtils";
import { txTypeColor, txTypeLabel, txSourceLabel, txStatusColor, txStatusLabel } from "@utility/billingUtils";

export const transactionDateColumn = (): TableColumnDefinition<BillingTransaction> =>
    createTableColumn<BillingTransaction>({
        columnId: "date",
        compare: (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
        renderHeaderCell: () => "Дата",
        renderCell: (tx) => <TableCellLayout truncate>{formatDateTime(tx.createdAt)}</TableCellLayout>,
    });

export const transactionTypeColumn = (): TableColumnDefinition<BillingTransaction> =>
    createTableColumn<BillingTransaction>({
        columnId: "type",
        compare: (a, b) => a.type - b.type,
        renderHeaderCell: () => "Тип",
        renderCell: (tx) => (
            <TableCellLayout truncate>
                <Badge appearance="tint" color={txTypeColor(tx.type)}>{txTypeLabel(tx.type)}</Badge>
            </TableCellLayout>
        ),
    });

export const transactionSourceColumn = (): TableColumnDefinition<BillingTransaction> =>
    createTableColumn<BillingTransaction>({
        columnId: "source",
        compare: (a, b) => a.source - b.source,
        renderHeaderCell: () => "Источник",
        renderCell: (tx) => <TableCellLayout truncate>{txSourceLabel(tx.source)}</TableCellLayout>,
    });

export const transactionStatusColumn = (): TableColumnDefinition<BillingTransaction> =>
    createTableColumn<BillingTransaction>({
        columnId: "status",
        compare: (a, b) => a.status - b.status,
        renderHeaderCell: () => "Статус",
        renderCell: (tx) => (
            <TableCellLayout truncate>
                <Badge appearance="tint" color={txStatusColor(tx.status)}>{txStatusLabel(tx.status)}</Badge>
            </TableCellLayout>
        ),
    });

export const transactionAmountColumn = (): TableColumnDefinition<BillingTransaction> =>
    createTableColumn<BillingTransaction>({
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
    });

export const transactionBalanceColumn = (): TableColumnDefinition<BillingTransaction> =>
    createTableColumn<BillingTransaction>({
        columnId: "balance",
        compare: (a, b) => a.balanceAfter - b.balanceAfter,
        renderHeaderCell: () => "Баланс после",
        renderCell: (tx) => <TableCellLayout truncate>{formatMoney(tx.balanceAfter)}</TableCellLayout>,
    });

export const transactionCommentColumn = (): TableColumnDefinition<BillingTransaction> =>
    createTableColumn<BillingTransaction>({
        columnId: "comment",
        compare: (a, b) => (a.comment ?? "").localeCompare(b.comment ?? ""),
        renderHeaderCell: () => "Комментарий",
        renderCell: (tx) => <TableCellLayout truncate>{tx.comment || NO_DATA}</TableCellLayout>,
    });

export const withPermission = <T,>(
    column: TableColumnDefinition<T>,
    permission: string
): TableColumnDefinition<T> & { permission: string } => ({
    ...column,
    permission,
});
