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

export const TransactionsSection = () => {
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

    const transactions: Transaction[] = [
        {
            id: "1",
            icon: "☕",
            title: "Визит: Главный зал",
            sub: "Сегодня, 14:20 • 2ч 15мин",
            amount: 525,
            status: "Завершено",
        },
        {
            id: "2",
            icon: <Add20Regular/>,
            title: "Пополнение баланса",
            sub: "Вчера, 18:10 • Stripe",
            amount: 2000,
            status: "Успешно",
            isPositive: true,
        },
    ];

    return (
        <Card className="flex flex-col gap-4 h-full">
            <div className="flex items-center justify-between">
                <Title1>История операций</Title1>
                <TooltipButton appearance="subtle" size="small" tooltip="Фильтры (скоро)" label="Фильтры"/>
            </div>

            <div className="overflow-hidden">
                <DataTable items={transactions} columns={columns} getRowId={(item) => item.id}/>
            </div>

            <TooltipButton
                appearance="outline"
                tooltip="Открыть полную историю (скоро)"
                label="Загрузить все транзакции"
            />
        </Card>
    );
};
