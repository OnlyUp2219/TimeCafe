import { useMemo } from "react";
import {
    Card,
    Title2,
    type TableColumnDefinition,
    type TableColumnSizingOptions,
    Body1
} from "@fluentui/react-components";

import { type BillingTransaction } from "@app-types/billing";

import { DataTable } from "@components/DataTable";
import { useComponentSize } from "@hooks/useComponentSize";
import { Pagination } from "@components/Pagination";
import {
    transactionDateColumn,
    transactionTypeColumn,
    transactionSourceColumn,
    transactionStatusColumn,
    transactionAmountColumn,
    transactionCommentColumn,
} from "@components/Billing/TransactionColumns";

type TransactionsSectionProps = {
    transactions: BillingTransaction[];
    loading: boolean;
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
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
        transactionDateColumn(),
        transactionTypeColumn(),
        transactionSourceColumn(),
        transactionStatusColumn(),
        transactionAmountColumn(),
        transactionCommentColumn(),
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
