import {BillingType as BillingTypeEnum, type Tariff} from "@app-types/tariff";

export const formatRub = (value: number, maximumFractionDigits = 0) => {
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

export const formatMinutesAsDuration = (minutes: number) => {
    const safeMinutes = Math.max(0, Math.floor(minutes));
    const hours = Math.floor(safeMinutes / 60);
    const restMinutes = safeMinutes % 60;

    if (hours <= 0) {
        return `${restMinutes}мин`;
    }

    return `${hours}ч ${restMinutes.toString().padStart(2, "0")}мин`;
};

export type MockActivityPoint = {
    date: Date;
    depositsRub: number;
    withdrawalsRub: number;
};

export const mockTariffs: Tariff[] = [
    {
        tariffId: "standard",
        name: "Стандарт",
        description: "Обычный тариф",
        billingType: BillingTypeEnum.PerMinute,
        pricePerMinute: 7,
        isActive: true,
        themeEmoji: "☕",
    },
    {
        tariffId: "discount",
        name: "Льготный",
        description: "Для студентов и постоянных гостей",
        billingType: BillingTypeEnum.PerMinute,
        pricePerMinute: 5,
        isActive: true,
        themeEmoji: "🎓",
    },
];

export const mockBilling = {
    balanceRub: 3500,
    debtRub: 200,
    lastVisitTariffId: null as string | null,
};

export const mockWeeklyActivity: MockActivityPoint[] = [
    ...(() => {
        const today = new Date();

        today.setHours(12, 0, 0, 0);

        const amounts = [
            {depositsRub: 1500, withdrawalsRub: 0},
            {depositsRub: 2200, withdrawalsRub: 0},
            {depositsRub: 800, withdrawalsRub: 600},
            {depositsRub: 3100, withdrawalsRub: 0},
            {depositsRub: 1900, withdrawalsRub: 0},
            {depositsRub: 4500, withdrawalsRub: 0},
            {depositsRub: 2800, withdrawalsRub: 0},
        ];

        return amounts.map((a, idx) => {
            const date = new Date(today);
            date.setDate(today.getDate() - (6 - idx));
            return {date, ...a};
        });
    })(),
];
