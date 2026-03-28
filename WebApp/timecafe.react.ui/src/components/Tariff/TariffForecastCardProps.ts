import type {Tariff} from "@app-types/tariff";
import type {CalcResult} from "@components/Tariff/TariffForecastCard";

export type TariffForecastCardProps = {
    selectedTariff: Tariff | null;
    calc: CalcResult | null;
};