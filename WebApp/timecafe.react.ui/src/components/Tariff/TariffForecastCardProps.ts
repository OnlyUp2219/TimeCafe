import type {Tariff} from "@app-types/tariff.ts";
import type {CalcResult} from "@components/Tariff/TariffForecastCard.tsx";

export type TariffForecastCardProps = {
    selectedTariff: Tariff | null;
    calc: CalcResult | null;
};