import type { FC } from "react";
import { Body1, Subtitle1, Card, CardHeader, Divider, Text } from "@fluentui/react-components";
import { formatDateTime } from "@utility/dateUtils";
import { formatMoney } from "@utility/formatUtils";

export interface VirtualReceiptProps {
    receiptNumber: string;
    amount: number;
    date: string;
    tariffName?: string;
    visitId: string;
}

export const VirtualReceipt: FC<VirtualReceiptProps> = ({ receiptNumber, amount, date, tariffName, visitId }) => {
    return (
        <Card className="max-w-md mx-auto my-4 border-2 border-dashed rounded-sm shadow-md p-6 font-mono" style={{ borderColor: 'var(--colorNeutralStroke1)', backgroundColor: 'var(--colorNeutralBackground3)', color: 'var(--colorNeutralForeground1)' }}>
            <CardHeader
                header={
                    <div className="text-center w-full">
                        <Subtitle1 className="font-bold tracking-widest uppercase">TimeCafe</Subtitle1>
                        <br/>
                        <Text size={200} className="uppercase" style={{ color: 'var(--colorNeutralForeground3)' }}>Кассовый чек (Имитация)</Text>
                    </div>
                }
            />
            
            <div className="flex flex-col gap-2 mt-4 text-sm">
                <div className="flex justify-between">
                    <Text>Кассир:</Text>
                    <Text>Автоматическая система</Text>
                </div>
                <div className="flex justify-between">
                    <Text>Чек №:</Text>
                    <Text>{receiptNumber}</Text>
                </div>
                <div className="flex justify-between">
                    <Text>Дата/Время:</Text>
                    <Text>{formatDateTime(date)}</Text>
                </div>
                <div className="flex justify-between">
                    <Text>Визит ID:</Text>
                    <Text className="truncate max-w-[120px]" title={visitId}>{visitId.split('-')[0]}...</Text>
                </div>
            </div>

            <Divider className="my-4 border-dashed" />

            <div className="flex flex-col gap-2">
                <div className="flex justify-between font-bold">
                    <Text>Услуга</Text>
                    <Text>Сумма</Text>
                </div>
                <div className="flex justify-between">
                    <Text>{tariffName ? `Посещение: ${tariffName}` : "Оплата услуг тайм-кафе"}</Text>
                    <Text>{formatMoney(amount)}</Text>
                </div>
            </div>

            <Divider className="my-4 border-dashed" />

            <div className="flex justify-between items-center mt-2">
                <Body1 className="font-bold text-lg">ИТОГО</Body1>
                <Body1 className="font-bold text-lg">{formatMoney(amount)}</Body1>
            </div>

            <div className="mt-8 text-center text-xs uppercase tracking-widest" style={{ color: 'var(--colorNeutralForeground4)' }}>
                <Text>Спасибо за посещение!</Text>
                <br/>
                <Text>Фискальный документ сформирован в тестовом режиме</Text>
            </div>
        </Card>
    );
};
