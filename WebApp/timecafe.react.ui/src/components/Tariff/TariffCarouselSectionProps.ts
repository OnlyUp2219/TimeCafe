import type {Tariff} from "@app-types/tariff.ts";

export type TariffCarouselSectionProps = {
    visibleTariffs: Tariff[];
    totalCount: number;
    activeIndex: number;
    onActiveIndexChange: (nextIndex: number) => void;
    selectedTariffId: string | null;
    onSelectTariff: (tariffId: string) => void;
};