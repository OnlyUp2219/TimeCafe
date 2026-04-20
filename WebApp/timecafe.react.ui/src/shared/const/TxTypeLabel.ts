import {TransactionType} from "@app/types/billing.ts";

export const txTypeLabel = (t: number) => {
    switch (t) {
        case TransactionType.Deposit:
            return "Пополнение";
        case TransactionType.Withdrawal:
            return "Списание";
        case TransactionType.Adjustment:
            return "Корректировка";
        default:
            return "—";
    }
};