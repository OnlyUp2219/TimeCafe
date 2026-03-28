import {formatMoneyByN} from "./formatMoney";

export const formatRub = (value: number, maximumFractionDigits = 0) =>
    formatMoneyByN(value, maximumFractionDigits);