export const formatMoneyByN = (value: number, maximumFractionDigits = 2) => {
    try {
        return new Intl.NumberFormat("ru-RU", {
            style: "currency",
            currency: "RUB",
            maximumFractionDigits,
        }).format(value);
    } catch {
        return `${value.toFixed(maximumFractionDigits)} ₽`;
    }
};
