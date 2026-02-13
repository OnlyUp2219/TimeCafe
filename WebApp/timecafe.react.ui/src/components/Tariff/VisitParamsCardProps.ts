import type {Tariff} from "@app-types/tariff.ts";

export type VisitParamsCardProps = {
    selectedTariff: Tariff | null;
    durationMinutes: number;
    setDurationMinutes: (value: number) => void;
    presets: number[];
};