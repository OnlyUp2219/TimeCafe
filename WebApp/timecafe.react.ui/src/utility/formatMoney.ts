export const formatMoneyByN = (value: number, maximumFractionDigits = 2) => {
    try {
        return new Intl.NumberFormat("ru-BY", {
            style: "currency",
            currency: "BYN",
            maximumFractionDigits,
        }).format(value);
    } catch {
        return `${value.toFixed(maximumFractionDigits)} BYN`;
    }
};
