import type {VisitParamsCardProps} from "@components/Tariff/VisitParamsCardProps";
import {Body2, Button, Card, Divider, Field, Input, Tag, Title3, Select} from "@fluentui/react-components";
import {clamp} from "@utility/clamp";
import {formatDurationMinutes} from "@utility/formatDurationMinutes";
import {BillingType as BillingTypeEnum, type BillingType} from "@app-types/tariff";
import {useComponentSize} from "@hooks/useComponentSize";

const getBillingLabel = (billingType: BillingType) =>
    billingType === BillingTypeEnum.PerMinute ? "Оплата за минуты" : "Округление до часа";

export const VisitParamsCard = ({
                                    selectedTariff,
                                    durationMinutes,
                                    setDurationMinutes,
                                    presets,
                                    guestsCount,
                                    setGuestsCount,
                                    resources = [],
                                    selectedResourceId = null,
                                    setSelectedResourceId
                                }: VisitParamsCardProps) => {
    const { sizes } = useComponentSize();
    const maxGuestsLimit = selectedTariff?.maxGuests ?? 10;

    return (
        <Card className="lg:col-span-7" size={sizes.card}>
            <div className="flex flex-col gap-4">
                <div className="flex items-center justify-between gap-3 flex-wrap">
                    <Title3>Параметры визита</Title3>
                </div>
                <Divider/>
 
                {selectedTariff ? (
                    <div className="flex flex-col gap-4">
                        <Field
                            label="Примерное время пребывания"
                            hint="Можно выбрать пресет или ввести вручную (в минутах)"
                            size={sizes.field}
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
                                size={sizes.input}
                            />
                        </Field>
 
                        <div className="flex flex-wrap gap-2">
                            {presets.map((m) => (
                                <Button
                                    key={m}
                                    appearance={durationMinutes === m ? "primary" : "secondary"}
                                    onClick={() => setDurationMinutes(m)}
                                    size={sizes.button}
                                >
                                    {formatDurationMinutes(m)}
                                </Button>
                            ))}
                        </div>

                        {resources && resources.length > 0 && setSelectedResourceId && (
                            <Field
                                label="Столик/Зона"
                                hint="Выберите свободный столик"
                                size={sizes.field}
                            >
                                <Select
                                    value={selectedResourceId ?? ""}
                                    onChange={(_, data) => setSelectedResourceId(data.value || null)}
                                    size={sizes.input}
                                >
                                    <option value="">Не выбран (Любой свободный)</option>
                                    {resources.map((res) => (
                                        <option key={res.resourceId} value={res.resourceId}>
                                            {res.name} (мест: {res.capacity})
                                        </option>
                                    ))}
                                </Select>
                            </Field>
                        )}

                        <Field
                            label="Количество гостей"
                            hint={`Введите количество гостей (от 1 до ${maxGuestsLimit})`}
                            size={sizes.field}
                        >
                            <Input
                                type="number"
                                value={String(guestsCount)}
                                onChange={(_, data) => {
                                    const next = Number(data.value);
                                    if (!Number.isFinite(next)) return;
                                    setGuestsCount(clamp(next, 1, maxGuestsLimit));
                                }}
                                min={1}
                                max={maxGuestsLimit}
                                size={sizes.input}
                            />
                        </Field>
 
                        <div className="flex flex-wrap items-center gap-2">
                            <Tag appearance="brand" size="small">{selectedTariff.name}</Tag>
                            <Tag appearance="outline" size="small">{getBillingLabel(selectedTariff.billingType)}</Tag>
                            <Tag appearance="outline" size="small">{formatDurationMinutes(durationMinutes)}</Tag>
                            <Tag appearance="outline" size="small">{guestsCount} {guestsCount === 1 ? "гость" : guestsCount < 5 ? "гостя" : "гостей"}</Tag>
                        </div>
                    </div>
                ) : (
                    <div className="flex">
                        <Body2>Сначала выберите тариф в карусели.</Body2>
                    </div>
                )}
            </div>
        </Card>
    );
};