export const formatRub = (value: number, maximumFractionDigits = 0) => {
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