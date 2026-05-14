import { useState, useMemo, useEffect, useDeferredValue } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
    Body2,
    Button,
    Card,
    Field,
    Input,
    MessageBar,
    MessageBarBody,
    Spinner,
    Title1,
    Title2,
    Tooltip,
    Dropdown,
    Option,
    Switch,
    Caption1,
    Subtitle2,
    Divider,
    TagPicker,
    TagPickerInput,
    TagPickerControl,
    TagPickerGroup,
    Tag
} from "@fluentui/react-components";
import { ArrowLeft20Regular, Save20Regular, Dismiss20Regular } from "@fluentui/react-icons";
import {
    useGetAllThemesQuery,
    useGetTariffByIdQuery,
    useCreateTariffMutation,
    useUpdateTariffMutation,
} from "@store/api/venueApi";
import { TariffCard } from "@components/TariffCard/TariffCard";
import { type Tariff } from "@app-types/tariff";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { CURRENCY_SYMBOL } from "@shared/const/currency";
import { TextareaWithCounter } from "@components/FormFields";
import { useComponentSize } from "@hooks/useComponentSize";

interface TariffFormState {
    name: string;
    description: string;
    pricePerMinute: string;
    billingType: "1" | "2";
    themeId: string;
    isActive: boolean;
    summary: string;
    features: string[];
    audienceTags: string[];
    minSessionMinutes: string;
    roundingRule: string;
    maxGuests: string;
    cancellationPolicy: string;
    isRecommended: boolean;
    sortOrder: string;
}

const emptyForm: TariffFormState = {
    name: "",
    description: "",
    pricePerMinute: "",
    billingType: "2",
    themeId: "",
    isActive: true,
    summary: "",
    features: [],
    audienceTags: [],
    minSessionMinutes: "",
    roundingRule: "None",
    maxGuests: "",
    cancellationPolicy: "",
    isRecommended: false,
    sortOrder: "0",
};

const calcPerHour = (perMinute: string): string => {
    const v = Number.parseFloat(perMinute);
    if (Number.isNaN(v)) return "";
    return (v * 60).toFixed(2);
};

const TagPickerNoPopover = ({ label, value, onChange, placeholder }: { label: string, value: string[], onChange: (val: string[]) => void, placeholder?: string }) => {
    const [inputValue, setInputValue] = useState("");

    const handleKeyDown = (event: React.KeyboardEvent) => {
        if (event.key === "Enter" && inputValue) {
            event.preventDefault();
            setInputValue("");
            onChange(value.includes(inputValue) ? value : [...value, inputValue]);
        }
    };

    return (
        <Field label={label} size="large">
            <TagPicker
                noPopover
                selectedOptions={value}
                onOptionSelect={(_, data) => onChange(data.selectedOptions)}
            >
                <TagPickerControl>
                    <TagPickerGroup aria-label={label}>
                        {value.map((option) => (
                            <Tag
                                key={option}
                                shape="rounded"
                                value={option}
                                onClick={() => onChange(value.filter(v => v !== option))}
                            >
                                {option}
                            </Tag>
                        ))}
                    </TagPickerGroup>
                    <TagPickerInput
                        value={inputValue}
                        onChange={(e) => setInputValue(e.target.value)}
                        onKeyDown={handleKeyDown}
                        aria-label={label}
                        placeholder={placeholder}
                    />
                </TagPickerControl>
            </TagPicker>
        </Field>
    );
};

const roundingRules = [
    { value: "None", text: "Без округления" },
    { value: "ToNearestFive", text: "До 5 минут" },
    { value: "ToNearestTen", text: "До 10 минут" },
    { value: "ToNearestHalfHour", text: "До 30 минут" },
    { value: "ToNearestHour", text: "До 1 часа" },
];

const billingTypes = [
    { value: "2", text: "Поминутный" },
    { value: "1", text: "Почасовой" },
];

export const TariffEditorPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { sizes } = useComponentSize();
    const { data: themes = [], isLoading: loadingThemes } = useGetAllThemesQuery();
    const { data: tariff, isLoading: loadingTariff } = useGetTariffByIdQuery(id!, { skip: !id });
    const [createTariff, { isLoading: creating }] = useCreateTariffMutation();
    const [updateTariff, { isLoading: updating }] = useUpdateTariffMutation();

    const [form, setForm] = useState<TariffFormState>(emptyForm);
    const [error, setError] = useState<string | null>(null);

    const deferredForm = useDeferredValue(form);

    useEffect(() => {
        if (id && tariff) {
            setForm({
                name: tariff.name,
                description: tariff.description ?? "",
                pricePerMinute: String(tariff.pricePerMinute),
                billingType: String(tariff.billingType) as "1" | "2",
                themeId: tariff.themeId ?? "",
                isActive: tariff.isActive,
                summary: tariff.summary ?? "",
                features: tariff.features ?? [],
                audienceTags: tariff.audienceTags ?? [],
                minSessionMinutes: tariff.minSessionMinutes ? String(tariff.minSessionMinutes) : "",
                roundingRule: tariff.roundingRule ?? "None",
                maxGuests: tariff.maxGuests ? String(tariff.maxGuests) : "",
                cancellationPolicy: tariff.cancellationPolicy ?? "",
                isRecommended: tariff.isRecommended ?? false,
                sortOrder: tariff.sortOrder ? String(tariff.sortOrder) : "0",
            });
        }
    }, [id, tariff]);

    const handleSave = async () => {
        setError(null);

        const price = Number.parseFloat(form.pricePerMinute);
        if (Number.isNaN(price) || price < 0) {
            setError("Цена за минуту должна быть корректным числом >= 0");
            return;
        }

        const payload = {
            name: form.name,
            description: form.description,
            pricePerMinute: price,
            billingType: Number.parseInt(form.billingType, 10) as 1 | 2,
            themeId: form.themeId || undefined,
            isActive: form.isActive,
            summary: form.summary,
            features: form.features,
            audienceTags: form.audienceTags,
            minSessionMinutes: form.minSessionMinutes ? Number.parseInt(form.minSessionMinutes, 10) : undefined,
            roundingRule: form.roundingRule,
            maxGuests: form.maxGuests ? Number.parseInt(form.maxGuests, 10) : undefined,
            cancellationPolicy: form.cancellationPolicy,
            isRecommended: form.isRecommended,
            sortOrder: Number.parseInt(form.sortOrder, 10) || 0,
        };

        try {
            if (id) {
                await updateTariff({ tariffId: id, ...payload }).unwrap();
            } else {
                await createTariff(payload).unwrap();
            }
            navigate("/admin/tariffs");
        } catch (err) {
            setError(getRtkErrorMessage(err as FetchBaseQueryError) || "Ошибка при сохранении");
        }
    };

    const selectedTheme = useMemo(() => {
        return themes.find(t => t.themeId === form.themeId);
    }, [themes, form.themeId]);

    const mockTariff = useMemo((): Tariff => {
        const price = Number.parseFloat(deferredForm.pricePerMinute) || 0;
        return {
            tariffId: id || "preview",
            name: deferredForm.name || "Название тарифа",
            description: deferredForm.description || "Описание тарифа",
            pricePerMinute: price,
            billingType: Number.parseInt(deferredForm.billingType, 10) as 1 | 2,
            isActive: deferredForm.isActive,
            summary: deferredForm.summary,
            features: deferredForm.features,
            audienceTags: deferredForm.audienceTags,
            minSessionMinutes: deferredForm.minSessionMinutes ? Number.parseInt(deferredForm.minSessionMinutes, 10) : null,
            roundingRule: deferredForm.roundingRule,
            maxGuests: deferredForm.maxGuests ? Number.parseInt(deferredForm.maxGuests, 10) : null,
            cancellationPolicy: deferredForm.cancellationPolicy,
            isRecommended: deferredForm.isRecommended,
            sortOrder: Number.parseInt(deferredForm.sortOrder, 10) || 0,
            themeId: deferredForm.themeId || null,
            themeColors: selectedTheme?.colors,
            themeEmoji: selectedTheme?.emoji,
        };
    }, [deferredForm, selectedTheme, id]);

    const perHour = calcPerHour(form.pricePerMinute);

    if (id && loadingTariff) return <div className="flex justify-center p-12"><Spinner label="Загрузка тарифа..." /></div>;
    if (loadingThemes) return <div className="flex justify-center p-12"><Spinner label="Загрузка тем..." /></div>;

    const currentBillingType = billingTypes.find(t => t.value === form.billingType);
    const currentRoundingRule = roundingRules.find(r => r.value === form.roundingRule);

    return (
        <div className="flex flex-col gap-4">
            <div className="flex items-center justify-between sticky z-30 flex-wrap gap-2">
                <div className="flex items-center gap-4">
                    <Tooltip content="Назад" relationship="label">
                        <Button
                            appearance="subtle"
                            size={sizes.button}
                            icon={<ArrowLeft20Regular />}
                            onClick={() => navigate("/admin/tariffs")}
                        />
                    </Tooltip>
                    <div>
                        <Title1>{id ? "Редактирование тарифа" : "Новый тариф"}</Title1>
                    </div>
                </div>
                <div className="flex gap-2">
                    <Tooltip content="Отменить и вернуться" relationship="label">
                        <Button
                            appearance="secondary"
                            size={sizes.button}
                            onClick={() => navigate("/admin/tariffs")}
                            icon={<Dismiss20Regular />}
                        >
                            Отмена
                        </Button>
                    </Tooltip>
                    <Tooltip content="Сохранить все изменения" relationship="label">
                        <Button
                            appearance="primary"
                            size={sizes.button}
                            onClick={handleSave}
                            disabled={creating || updating}
                            icon={<Save20Regular />}
                        >
                            Сохранить
                        </Button>
                    </Tooltip>
                </div>
            </div>
            <Divider />

            {error && (
                <MessageBar intent="error">
                    <MessageBarBody>{error}</MessageBarBody>
                </MessageBar>
            )}

            <div className="flex gap-4 flex-wrap">
                <div className="flex flex-col gap-4 flex-1">

                    <div className="flex flex-col gap-4">
                        <Title2>Основная информация</Title2>

                        <Field label="Название" required size={sizes.field}>
                            <Input
                                size={sizes.input}
                                value={form.name}
                                autoComplete="off"
                                onChange={(_, d) => setForm(f => ({ ...f, name: d.value }))}
                            />
                        </Field>

                        <TextareaWithCounter
                            label="Описание"
                            value={form.description}
                            onChange={(val) => setForm(f => ({ ...f, description: val }))}
                            maxLength={2000}
                            placeholder="Полное описание тарифа..."
                            rows={4}
                            fieldSize={sizes.field}
                        />

                        <TextareaWithCounter
                            label="Краткая сводка (Summary)"
                            value={form.summary}
                            onChange={(val) => setForm(f => ({ ...f, summary: val }))}
                            maxLength={1000}
                            placeholder="Краткое описание тарифа..."
                            rows={2}
                            fieldSize={sizes.field}
                        />

                        <div className="flex flex-wrap gap-4">
                            <Field label={`Цена за минуту (${CURRENCY_SYMBOL})`} required size={sizes.field} hint={perHour ? `≈ ${perHour} ${CURRENCY_SYMBOL}/час` : undefined} className="flex-[1]">
                                <Input
                                    type="number"
                                    size={sizes.input}
                                    value={form.pricePerMinute}
                                    autoComplete="off"
                                    onChange={(_, d) => setForm(f => ({ ...f, pricePerMinute: d.value }))}
                                    min={0}
                                />
                            </Field>

                            <Field label="Тип тарификации" required size={sizes.field} className="flex-[1]">
                                <Dropdown
                                    size={sizes.input}
                                    value={currentBillingType?.text || "Поминутный"}
                                    selectedOptions={[form.billingType]}
                                    onOptionSelect={(_, d) => setForm(f => ({ ...f, billingType: (d.selectedOptions[0] || "2") as "1" | "2" }))}
                                >
                                    {billingTypes.map(t => (
                                        <Option key={t.value} value={t.value} text={t.text}>
                                            {t.text}
                                        </Option>
                                    ))}
                                </Dropdown>
                            </Field>
                        </div>
                    </div>

                    <Divider />

                    <div className="flex flex-col gap-4">
                        <Title2>Дополнительные параметры</Title2>

                        <div className="flex flex-wrap gap-4">
                            <Field label="Минимум минут сессии" size={sizes.field} className="flex-1">
                                <Input
                                    type="number"
                                    size={sizes.input}
                                    value={form.minSessionMinutes}
                                    autoComplete="off"
                                    onChange={(_, d) => setForm(f => ({ ...f, minSessionMinutes: d.value }))}
                                    placeholder="Например: 15"
                                    min={0}
                                />
                            </Field>

                            <Field label="Правило округления" size={sizes.field} className="flex-1">
                                <Dropdown
                                    size={sizes.input}
                                    value={currentRoundingRule?.text || "Без округления"}
                                    selectedOptions={[form.roundingRule]}
                                    onOptionSelect={(_, d) => setForm(f => ({ ...f, roundingRule: d.selectedOptions[0] || "None" }))}
                                >
                                    {roundingRules.map(r => (
                                        <Option key={r.value} value={r.value} text={r.text}>
                                            {r.text}
                                        </Option>
                                    ))}
                                </Dropdown>
                            </Field>

                            <Field label="Макс. гостей" size={sizes.field} className="flex-1">
                                <Input
                                    type="number"
                                    size={sizes.input}
                                    value={form.maxGuests}
                                    autoComplete="off"
                                    onChange={(_, d) => setForm(f => ({ ...f, maxGuests: d.value }))}
                                    placeholder="Без ограничений"
                                    min={0}
                                />
                            </Field>

                            <Field label="Порядок сортировки" size={sizes.field} className="flex-1">
                                <Input
                                    type="number"
                                    size={sizes.input}
                                    value={form.sortOrder}
                                    autoComplete="off"
                                    onChange={(_, d) => setForm(f => ({ ...f, sortOrder: d.value }))}
                                    min={0}
                                />
                            </Field>
                        </div>

                        <TagPickerNoPopover
                            label="Фичи"
                            value={form.features}
                            onChange={(val) => setForm(f => ({ ...f, features: val }))}
                            placeholder="Нажмите Enter для добавления"
                        />

                        <TagPickerNoPopover
                            label="Теги аудитории"
                            value={form.audienceTags}
                            onChange={(val) => setForm(f => ({ ...f, audienceTags: val }))}
                            placeholder="Нажмите Enter для добавления"
                        />

                        <TextareaWithCounter
                            label="Политика отмены"
                            value={form.cancellationPolicy}
                            onChange={(val) => setForm(f => ({ ...f, cancellationPolicy: val }))}
                            maxLength={1000}
                            placeholder="Правила возврата при отмене бронирования"
                            rows={3}
                            fieldSize={sizes.field}
                        />
                    </div>

                    <Divider />

                    <div className="flex flex-col gap-4">
                        <Title2>Визуализация и статус</Title2>

                        <Field label="Визуальная тема" size={sizes.field}>
                            <Dropdown
                                size={sizes.input}
                                value={selectedTheme ? `${selectedTheme.emoji} ${selectedTheme.name}` : "Без темы"}
                                selectedOptions={[form.themeId]}
                                onOptionSelect={(_, d) => setForm(f => ({ ...f, themeId: d.selectedOptions[0] || "" }))}
                            >
                                <Option value="" text="Без темы">Без темы</Option>
                                {themes.map(t => (
                                    <Option key={t.themeId} value={t.themeId} text={`${t.emoji} ${t.name}`}>
                                        {t.emoji} {t.name}
                                    </Option>
                                ))}
                            </Dropdown>
                        </Field>

                        <div className="flex gap-6">
                            <Switch
                                label="Активен"
                                checked={form.isActive}
                                onChange={(_, d) => setForm(f => ({ ...f, isActive: d.checked }))}
                            />
                            <Switch
                                label="Рекомендуемый"
                                checked={form.isRecommended}
                                onChange={(_, d) => setForm(f => ({ ...f, isRecommended: d.checked }))}
                            />
                        </div>
                    </div>
                </div>

                <div
                    className="flex flex-col gap-4 flex-1 pl-2 pr-2"
                    style={{ backgroundColor: "var(--colorNeutralBackground2)" }}
                >
                    <div className="flex flex-col">
                        <Body2 className="uppercase text-[var(--colorNeutralForeground4)]">Предпросмотр</Body2>
                        <Body2 className="text-[var(--colorNeutralForeground3)]">Живой результат</Body2>
                    </div>

                    <div className="flex flex-col gap-4 w-full h-full justify-center items-center content-center">
                        <TariffCard tariff={mockTariff} />

                        <Card className="w-[320px]">
                            <Subtitle2 block className="mb-2 font-bold">Справка</Subtitle2>
                            <Caption1 className="text-[var(--colorNeutralForeground2)] leading-relaxed italic block">
                                Предпросмотр показывает, как тариф будет выглядеть в карусели выбора тарифов для гостей.
                            </Caption1>
                        </Card>
                    </div>
                </div>
            </div>
        </div>
    );
};
