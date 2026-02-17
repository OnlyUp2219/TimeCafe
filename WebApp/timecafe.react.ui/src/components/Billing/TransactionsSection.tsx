import {
    Card,
    Caption1,
    Subtitle2Stronger,
    Title1,
    type TableColumnDefinition,
    createTableColumn,
    Avatar,
} from "@fluentui/react-components";

import {Add20Regular} from "@fluentui/react-icons";

import type {ReactElement} from "react";
import {
    TransactionSource,
    TransactionStatus,
    TransactionType,
    type BillingTransaction,
} from "@app-types/billing";
import {formatRub} from "@pages/billing/billing.mock";

import {DataTable} from "@components/DataTable";
import {TooltipButton} from "@components/TooltipButton/TooltipButton";

interface Transaction {
    id: string;
    icon: string | ReactElement;
    title: string;
    sub: string;
    amount: number;
    status: string;
    isPositive?: boolean;
}

type TransactionsSectionProps = {
    transactions: BillingTransaction[];
    loading: boolean;
    loadingMore: boolean;
    canLoadMore: boolean;
    onLoadMore: () => void;
};

const mapStatusLabel = (status: number): string => {
    if (status === TransactionStatus.Pending) return "В обработке";
    if (status === TransactionStatus.Completed) return "Успешно";
    if (status === TransactionStatus.Failed) return "Ошибка";
    if (status === TransactionStatus.PartialCompleted) return "Частично";
    return "Неизвестно";
};

const mapIcon = (transaction: BillingTransaction): string | ReactElement => {
    if (transaction.type === TransactionType.Deposit) {
        return <Add20Regular />;
    }

    if (transaction.source === TransactionSource.Visit) {
        return "☕";
    }

    if (transaction.source === TransactionSource.Refund) {
        return "↩";
    }

    return "₽";
};

const mapTitle = (transaction: BillingTransaction): string => {
    if (transaction.type === TransactionType.Deposit) return "Пополнение баланса";
    if (transaction.source === TransactionSource.Visit) return "Оплата визита";
    if (transaction.type === TransactionType.Adjustment) return "Корректировка баланса";
    return "Списание";
};

const mapSubtitle = (transaction: BillingTransaction): string => {
    const date = new Date(transaction.createdAt);
    const dateText = Number.isNaN(date.getTime())
        ? transaction.createdAt
        : date.toLocaleString("ru-RU", {
            day: "2-digit",
            month: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
        });

    return transaction.comment ? `${dateText} • ${transaction.comment}` : dateText;
};

export const TransactionsSection = ({
    transactions,
    loading,
    loadingMore,
    canLoadMore,
    onLoadMore,
}: TransactionsSectionProps) => {
    const columns: TableColumnDefinition<Transaction>[] = [
        createTableColumn<Transaction>({
            columnId: "info",
            renderHeaderCell: () => "Операция",
            renderCell: (item) => (
                <div className="flex items-center gap-3">
                    <Avatar
                        initials={typeof item.icon === "string" ? item.icon : undefined}
                        icon={typeof item.icon !== "string" ? item.icon : undefined}
                        aria-label={item.title}
                        color="neutral"
                        size={32}
                        shape="circular"
                    />
                    <div className="flex flex-col">
                        <Subtitle2Stronger block>{item.title}</Subtitle2Stronger>
                        <Caption1 block>{item.sub}</Caption1>
                    </div>
                </div>
            ),
        }),
        createTableColumn<Transaction>({
            columnId: "amount",
            renderHeaderCell: () => "Сумма",
            renderCell: (item) => (
                <div>
                    <Subtitle2Stronger block>
                        {item.isPositive ? "+" : ""}
                        {item.amount.toLocaleString()} ₽
                    </Subtitle2Stronger>
                    <Caption1>{item.status}</Caption1>
                </div>
            ),
        }),
    ];

    const tableItems: Transaction[] = transactions.map((transaction) => ({
        id: transaction.transactionId,
        icon: mapIcon(transaction),
        title: mapTitle(transaction),
        sub: mapSubtitle(transaction),
        amount: Math.abs(Number(transaction.amount) || 0),
        status: mapStatusLabel(transaction.status),
        isPositive: transaction.type === TransactionType.Deposit,
    }));

    return (
        <Card className="flex flex-col gap-4 h-full">
            <div className="flex items-center justify-between">
                <Title1>История операций</Title1>
                <Caption1>
                    Всего: {formatRub(tableItems.reduce((acc, item) => acc + (item.isPositive ? item.amount : -item.amount), 0), 0)}
                </Caption1>
            </div>

            <div className="overflow-hidden">
                <DataTable
                    items={tableItems}
                    columns={columns}
                    getRowId={(item) => item.id}
                    loading={loading}
                    emptyMessage="Транзакции пока отсутствуют"
                />
            </div>

            <TooltipButton
                appearance="outline"
                tooltip="Загрузить следующую страницу"
                label="Загрузить все транзакции"
                onClick={onLoadMore}
                disabled={!canLoadMore || loadingMore}
            />
        </Card>
    );
};
