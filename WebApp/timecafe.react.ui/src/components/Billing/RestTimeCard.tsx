import { useState } from "react";
import { Caption1, Card, Title2, Dropdown, Option, Input, Label } from "@fluentui/react-components";
import { formatDurationMinutes } from "@utility/formatDurationMinutes";
import { formatRub } from "@utility/formatMoney";
import type { Tariff } from "@app-types/tariff";
import { useComponentSize } from "@hooks/useComponentSize";
import "@pages/billing/billing.css";

type TariffWithDiscount = Tariff & { discountedPricePerMinute?: number };

type RestTimeCardProps = {
    availableRub: number;
    tariffs: TariffWithDiscount[];
    initialTariffId?: string;
};

export const RestTimeCard = ({ availableRub, tariffs, initialTariffId }: RestTimeCardProps) => {
    const { sizes } = useComponentSize();
    const [selectedId, setSelectedId] = useState<string | undefined>(initialTariffId);
    const [extraAmountStr, setExtraAmountStr] = useState<string>("");

    const extraAmount = Math.max(0, Number(extraAmountStr) || 0);
    const totalRub = Math.max(0, availableRub) + extraAmount;

    const selectedTariff = tariffs.find(t => t.tariffId === selectedId) || tariffs[0];
    const actualPricePerMinute = selectedTariff?.discountedPricePerMinute ?? selectedTariff?.pricePerMinute ?? 0;
    const minutes = actualPricePerMinute > 0 ? Math.floor(totalRub / actualPricePerMinute) : 0;

    return (
        <Card className="flex h-full flex-col justify-between gap-4 tc-billing-rest-card" size={sizes.card}>
            <div>
                <Caption1 block className="opacity-80">
                    Хватит на отдых
                </Caption1>
                <Title2 block>
                    ~ {formatDurationMinutes(minutes)}
                </Title2>
            </div>

            {tariffs.length > 0 && (
                <div className="flex flex-col gap-3 mt-auto">
                    <div className="flex flex-col gap-1">
                        <Label size="small">Выберите тариф для расчёта</Label>
                        <Dropdown
                            size={sizes.dropdown}
                            value={selectedTariff?.name || "Неизвестно"}
                            selectedOptions={selectedTariff ? [selectedTariff.tariffId] : []}
                            onOptionSelect={(_, data) => setSelectedId(data.optionValue)}
                        >
                            {tariffs.map(t => {
                                const p = t.discountedPricePerMinute ?? t.pricePerMinute;
                                const isDiscounted = p < t.pricePerMinute;
                                return (
                                <Option key={t.tariffId} value={t.tariffId} text={t.name}>
                                    {t.name} ({isDiscounted ? `${formatRub(p, 2)}/мин (скидка)` : `${formatRub(t.pricePerMinute, 0)}/мин`})
                                </Option>
                                );
                            })}
                        </Dropdown>
                    </div>

                    <div className="flex flex-col gap-1">
                        <Label size="small">Если пополнить на (₽)</Label>
                        <Input
                            size={sizes.input}
                            type="number"
                            min={0}
                            placeholder="0"
                            value={extraAmountStr}
                            onChange={(_, data) => setExtraAmountStr(data.value)}
                        />
                    </div>
                </div>
            )}
        </Card>
    );
};
