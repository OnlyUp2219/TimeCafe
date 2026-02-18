export const formatRub = (value: number, maximumFractionDigits = 0) => {
    try {
        return new Intl.NumberFormat("ru-RU", {
            style: "currency",
            currency: "RUB",
            maximumFractionDigits,
        }).format(value);
    } catch {
        return `${value.toFixed(maximumFractionDigits)} RUB`;
    }
};