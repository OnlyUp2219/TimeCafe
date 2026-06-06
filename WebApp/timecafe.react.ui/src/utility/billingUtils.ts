import { NO_DATA } from "@shared/const/placeholders";

export const paymentStatusLabel = (s: number) => {
    switch (s) {
        case 1: return "Ожидание";
        case 2: return "Выполнен";
        case 3: return "Ошибка";
        case 4: return "Возврат";
        case 5: return "Отменён";
        default: return NO_DATA;
    }
};

export const paymentStatusColor = (s: number): "warning" | "success" | "danger" | "informative" => {
    switch (s) {
        case 1: return "warning";
        case 2: return "success";
        case 3: return "danger";
        case 4: return "informative";
        case 5: return "danger";
        default: return "warning";
    }
};

export const paymentMethodLabel = (m: number) => {
    switch (m) {
        case 1: return "Карта (POS-терминал)";
        case 2: return "Наличные";
        case 3: return "Онлайн (Stripe)";
        default: return NO_DATA;
    }
};

export const txTypeLabel = (t: number) => {
    switch (t) {
        case 1: return "Пополнение";
        case 2: return "Списание";
        case 3: return "Корректировка";
        default: return NO_DATA;
    }
};

export const txTypeColor = (t: number): "success" | "danger" | "informative" | "warning" => {
    switch (t) {
        case 1: return "success";
        case 2: return "danger";
        case 3: return "warning";
        default: return "informative";
    }
};

export const txStatusLabel = (s: number) => {
    switch (s) {
        case 1: return "Ожидание";
        case 2: return "Выполнена";
        case 3: return "Ошибка";
        case 4: return "Частично";
        default: return NO_DATA;
    }
};

export const txStatusColor = (s: number): "warning" | "success" | "danger" | "informative" => {
    switch (s) {
        case 1: return "warning";
        case 2: return "success";
        case 3: return "danger";
        case 4: return "informative";
        default: return "warning";
    }
};

export const txSourceLabel = (s: number) => {
    switch (s) {
        case 1: return "Визит";
        case 2: return "Вручную";
        case 3: return "Платёж";
        case 4: return "Возврат";
        default: return NO_DATA;
    }
};

export const getBalanceColor = (balance: number): "danger" | "warning" | "success" => {
    if (balance < 0) return "danger";
    if (balance === 0) return "warning";
    return "success";
};
