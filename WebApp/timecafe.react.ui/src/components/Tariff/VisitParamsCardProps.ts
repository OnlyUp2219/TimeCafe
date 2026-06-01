import type {Tariff} from "@app-types/tariff";

export type VisitParamsCardProps = {
    selectedTariff: Tariff | null;
    durationMinutes: number;
    setDurationMinutes: (value: number) => void;
    presets: number[];
    guestsCount: number;
    setGuestsCount: (value: number) => void;
};