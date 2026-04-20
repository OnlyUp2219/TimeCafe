import {CURRENCY_SYMBOL} from "@shared/const/currency.ts";

export const formatMoney = (v: number) => `${v.toFixed(2)} ${CURRENCY_SYMBOL}`;