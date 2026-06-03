import type {Tariff} from "@app-types/tariff";
import type {Resource} from "@store/api/venueApi";

export type VisitParamsCardProps = {
    selectedTariff: Tariff | null;
    durationMinutes: number;
    setDurationMinutes: (value: number) => void;
    presets: number[];
    guestsCount: number;
    setGuestsCount: (value: number) => void;
    resources?: Resource[];
    selectedResourceId?: string | null;
    setSelectedResourceId?: (value: string | null) => void;
};