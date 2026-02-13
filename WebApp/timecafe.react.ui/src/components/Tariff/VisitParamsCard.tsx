import type {VisitParamsCardProps} from "@components/Tariff/VisitParamsCardProps.ts";
import {Body2, Button, Card, Divider, Field, Input, Tag, Title3} from "@fluentui/react-components";
import {clamp} from "@utility/clamp.ts";
import {formatDurationMinutes} from "@utility/formatDurationMinutes.ts";
import {BillingType as BillingTypeEnum, type BillingType} from "@app-types/tariff.ts";

const getBillingLabel = (billingType: BillingType) =>
    billingType === BillingTypeEnum.PerMinute ? "Оплата за минуты" : "Округление до часа";

export const VisitParamsCard = ({
                                    selectedTariff,
                                    durationMinutes,
                                    setDurationMinutes,
                                    presets
                                }: VisitParamsCardProps) => {
    return (
        <Card className="lg:col-span-7">
            <div className="flex flex-col gap-4">
                <div className="flex items-center justify-between gap-3 flex-wrap">
                    <Title3>Параметры визита</Title3>
                </div>
                <Divider/>

                {!selectedTariff ? (
                    <Body2 block>Сначала выберите тариф в карусели.</Body2>
                ) : (
                    <div className="flex flex-col gap-4">
                        <Field
                            label="Примерное время пребывания"
                            hint="Можно выбрать пресет или ввести вручную (в минутах)"
                        >
                            <Input
                                type="number"
                                value={String(durationMinutes)}
                                onChange={(_, data) => {
                                    const next = Number(data.value);
                                    if (!Number.isFinite(next)) return;
                                    setDurationMinutes(clamp(next, 1, 12 * 60));
                                }}
                                min={1}
                                max={12 * 60}
                            />
                        </Field>

                        <div className="flex flex-wrap gap-2">
                            {presets.map((m) => (
                                <Button
                                    key={m}
                                    appearance={durationMinutes === m ? "primary" : "secondary"}
                                    onClick={() => setDurationMinutes(m)}
                                >
                                    {formatDurationMinutes(m)}
                                </Button>
                            ))}
                        </div>

                        <div className="flex flex-wrap items-center gap-2">
                            <Tag appearance="brand">{selectedTariff.name}</Tag>
                            <Tag appearance="outline">{getBillingLabel(selectedTariff.billingType)}</Tag>
                            <Tag appearance="outline">{formatDurationMinutes(durationMinutes)}</Tag>
                        </div>
                    </div>
                )}
            </div>
        </Card>
    );
};