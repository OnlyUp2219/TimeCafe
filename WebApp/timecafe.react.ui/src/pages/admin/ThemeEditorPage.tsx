import React, { useState, useMemo, useEffect, useDeferredValue, memo } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
    Body2,
    Caption1,
    Button,
    Card,
    Field,
    Input,
    MessageBar,
    MessageBarBody,
    CardFooter,
    CardHeader,
    Title2,
    Slider,
    Dropdown,
    Option,
    Divider,
    Spinner,
    Title1,
    Subtitle1,
    ColorPicker as FluentColorPicker,
    ColorSlider,
    AlphaSlider,
    ColorArea,
    Popover,
    PopoverSurface,
    PopoverTrigger,
    Tooltip,
} from "@fluentui/react-components";
import { ArrowLeft20Regular, Color20Regular, Add20Regular, Delete20Regular } from "@fluentui/react-icons";
import { tinycolor } from "@ctrl/tinycolor";
import {
    useGetAllThemesQuery,
    useCreateThemeMutation,
    useUpdateThemeMutation,
} from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { EmojiPicker } from "@components/EmojiPicker/EmojiPicker";
import { parseThemeConfig, getThemeStyles, getPatternStyles, type ThemeConfig } from "@utility/themeStyles";

const ColorPicker = memo(({ value, onChange }: { value: string; onChange: (val: string) => void }) => {
    const color = useMemo(() => tinycolor(value).toHsv(), [value]);

    const handleColorChange = (_: any, data: { color: any }) => {
        const tc = tinycolor(data.color);
        const newColor = (data.color.a ?? 1) < 1 
            ? tc.toHex8String() 
            : tc.toHexString();
        onChange(newColor);
    };

    const displayValue = useMemo(() => {
        const tc = tinycolor(value);
        return tc.getAlpha() < 1 ? tc.toHex8String() : tc.toHexString();
    }, [value]);

    return (
        <div className="flex items-center gap-2">
            <Popover trapFocus>
                <PopoverTrigger disableButtonEnhancement>
                    <div
                        className="w-10 h-8 border border-gray-200 cursor-pointer rounded shadow-sm transition-transform hover:scale-110"
                        style={{ backgroundColor: value }}
                    />
                </PopoverTrigger>
                <PopoverSurface >
                    <div className="flex flex-col gap-2">
                        <FluentColorPicker color={color} onColorChange={handleColorChange}>
                            <ColorArea
                                inputX={{ "aria-label": "Saturation" }}
                                inputY={{ "aria-label": "Brightness" }}
                            />
                            <div className="flex gap-3 items-center">
                                <div className="flex flex-col flex-1 gap-1">
                                    <ColorSlider aria-label="Hue" />
                                    <AlphaSlider aria-label="Alpha" />
                                </div>
                                <div
                                    className="w-12 h-12 rounded shadow-sm border border-gray-200"
                                    style={{ backgroundColor: value }}
                                />
                            </div>
                        </FluentColorPicker>
                        <div className="flex gap-2 items-center">
                           <Input 
                             size="small" 
                             value={displayValue} 
                             onChange={(_, d) => onChange(d.value)}
                             className="grow font-mono uppercase"
                           />
                        </div>
                    </div>
                </PopoverSurface>
            </Popover>
            <Caption1 className="font-mono text-[10px] uppercase">{displayValue}</Caption1>
        </div>
    );
});

const ThemePreviewCard = memo(({ config, emoji, name, textColor, className }: { config: ThemeConfig; emoji: string; name: string; textColor: string, className: string }) => {
    const styles = useMemo(() => getThemeStyles(config), [config]);
    const pStyles = useMemo(() => getPatternStyles(config), [config]);

    return (
        <Card
            className={className}
            style={{
                ...styles,
                minHeight: "240px",
            }}
        >
            <div style={pStyles} />
            <CardHeader
                className="relative z-10"
                image={<span style={{ fontSize: "36px" }}>{emoji}</span>}
                header={<Title2 style={{ color: textColor }}>{name || "Название тарифа"}</Title2>}
                description={
                    <Body2 style={{ color: textColor, opacity: 0.8 }}>
                        Здесь будет описание вашего тарифа. Используйте яркие темы для привлечения внимания гостей!
                    </Body2>
                }
            />

            <Divider style={{ color: textColor, opacity: 0.3 }} className="relative z-10" />

            <div className="flex flex-col gap-1 px-3 relative z-10">
                <Caption1 style={{ color: textColor, opacity: 0.7 }}>Стандартный</Caption1>
                <Title2 style={{ color: textColor }}>15.00 ₽ / мин</Title2>
            </div>

            <CardFooter className="relative z-10">
                <Button appearance="primary" style={{ backgroundColor: "rgba(255,255,255,0.2)", color: textColor, }}>
                    Выбрать тариф
                </Button>
            </CardFooter>
        </Card>
    );
});

const emptyConfig: ThemeConfig = {
    type: "gradient",
    colors: ["#1a1a2e", "#16213e"],
    angle: 135,
    textColor: "#ffffff",
    blur: 0,
    pattern: "none",
    patternColor: "#ffffff33",
    patternScale: 1,
    patternAngle: 0,
    patternSkewX: 0,
    patternSkewY: 0,
    patternTranslateX: 0,
    patternTranslateY: 0,
    patternOpacity: 0.5,
};

export const ThemeEditorPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { data: themes = [], isLoading: loadingThemes } = useGetAllThemesQuery();
    const [createTheme, { isLoading: creating }] = useCreateThemeMutation();
    const [updateTheme, { isLoading: updating }] = useUpdateThemeMutation();

    const [form, setForm] = useState({
        name: "",
        emoji: "🚀",
        config: emptyConfig
    });

    const [error, setError] = useState<string | null>(null);
    const deferredConfig = useDeferredValue(form.config);

    useEffect(() => {
        if (id && themes.length > 0) {
            const theme = themes.find(t => t.themeId === id);
            if (theme) {
                setForm({
                    name: theme.name,
                    emoji: theme.emoji || "🚀",
                    config: parseThemeConfig(theme.colors)
                });
            }
        }
    }, [id, themes]);

    const handleSave = async () => {
        setError(null);
        const payload = {
            name: form.name,
            emoji: form.emoji,
            colors: JSON.stringify(form.config)
        };

        try {
            if (id) {
                await updateTheme({ themeId: id, ...payload }).unwrap();
            } else {
                await createTheme(payload).unwrap();
            }
            navigate("/admin/themes");
        } catch (err) {
            setError(getRtkErrorMessage(err as FetchBaseQueryError) || "Ошибка при сохранении");
        }
    };

    if (id && loadingThemes) return <Spinner size="huge" label="Загрузка темы..." className="p-20" />;

    return (
        <div className="flex flex-col gap-4">
            <div className="flex items-center justify-between sticky z-30">
                <div className="flex items-center gap-4">
                    <Tooltip content="Назад" relationship="label">
                        <Button
                            appearance="subtle"
                            icon={<ArrowLeft20Regular />}
                            onClick={() => navigate("/admin/themes")}
                        />
                    </Tooltip>
                    <div>
                        <Title1>{id ? "Редактирование темы" : "Новая визуальная тема"}</Title1>
                    </div>
                </div>
                <div className="flex gap-2">
                    <Tooltip content="Отменить и вернуться" relationship="label">
                        <Button
                            size="large"
                            appearance="secondary"
                            onClick={() => navigate("/admin/themes")}>Отмена</Button>
                    </Tooltip>
                    <Tooltip content="Сохранить все изменения" relationship="label">
                        <Button
                            size="large"
                            appearance="primary"
                            icon={<Color20Regular />}
                            onClick={handleSave}
                            disabled={creating || updating || !form.name}
                        >
                            {creating || updating ? "Сохранение..." : "Сохранить тему"}
                        </Button>
                    </Tooltip>
                </div>

            </div>
            <Divider/>
            {error && (
                <MessageBar intent="error" >
                    <MessageBarBody>{error}</MessageBarBody>
                </MessageBar>
            )}

            <div className="flex gap-4 flex-wrap ">
                <div className="flex flex-col gap-4 flex-1">
                    <div className="flex flex-col gap-4">
                        <div className="flex gap-4 flex-wrap">
                            <Field label="Иконка" size="large">
                                <EmojiPicker
                                    selectedEmoji={form.emoji}
                                    onSelect={(e) => setForm(f => ({ ...f, emoji: e }))}
                                    size="large"
                                />
                            </Field>
                            <Field label="Название темы" required className="grow" size="large">
                                <Input
                                    value={form.name}
                                    placeholder="Напр. Звездное небо"
                                    onChange={(_, d) => setForm(f => ({ ...f, name: d.value }))}
                                    size="large"
                                />
                            </Field>
                        </div>


                            <Subtitle1 >Базовый стиль</Subtitle1>
                            <div className="flex gap-4" flex-wrap>
                                <Field label="Тип заливки" size="large">
                                    <Dropdown
                                        size="large"
                                        value={form.config.type === "gradient" ? "Градиент" : form.config.type === "mesh" ? "Мэш" : "Сплошной"}
                                        selectedOptions={[form.config.type]}
                                        onOptionSelect={(_, d) => setForm(f => ({
                                            ...f,
                                            config: { ...f.config, type: d.optionValue as any }
                                        }))}
                                    >
                                        <Option value="solid" text="Сплошной">Сплошной</Option>
                                        <Option value="gradient" text="Градиент">Градиент</Option>
                                        <Option value="mesh" text="Мэш (fffuel)">Мэш (fffuel)</Option>
                                    </Dropdown>
                                </Field>
                                <Field label="Цвет текста" size="large">
                                    <ColorPicker
                                        value={form.config.textColor}
                                        onChange={(val) => setForm(f => ({ ...f, config: { ...f.config, textColor: val } }))}
                                    />
                                </Field>
                            </div>

                            <div>
                                <Field label="Цвета палитры"  size="large">
                                    <div className="flex flex-wrap gap-4 items-center">
                                        {form.config.colors.map((c, i) => (
                                            <Card size="large" key={i} className="flex !flex-row ">
                                                <ColorPicker
                                                    value={c}
                                                    onChange={(val) => setForm(f => {
                                                        const newColors = [...f.config.colors];
                                                        newColors[i] = val;
                                                        return { ...f, config: { ...f.config, colors: newColors } };
                                                    })}
                                                />
                                                {form.config.colors.length > 1 && (
                                                    <Tooltip content="Удалить" relationship="label">
                                                        <Button
                                                            appearance="secondary"
                                                            icon={<Delete20Regular className="text-red-500" />}
                                                            onClick={() => setForm(f => ({
                                                                ...f,
                                                                config: { ...f.config, colors: f.config.colors.filter((_, idx) => idx !== i) }
                                                            }))}
                                                        />
                                                    </Tooltip>
                                                )}
                                            </Card>
                                        ))}
                                        {form.config.colors.length < 5 && (
                                            <Tooltip content="Добавить новый цвет" relationship="label">
                                                <Button
                                                    appearance="outline"
                                                    size="large"
                                                    icon={<Add20Regular />}
                                                    onClick={() => setForm(f => ({
                                                        ...f,
                                                        config: { ...f.config, colors: [...f.config.colors, "#ffffff"] }
                                                    }))}
                                                >
                                                    Добавить цвет
                                                </Button>
                                            </Tooltip>
                                        )}
                                    </div>
                                </Field>
                            </div>

                            <div className="flex flex-col gap-4 ">
                                {form.config.type === "gradient" && (
                                    <Field label={`Угол градиента: ${form.config.angle}°`} size="large">
                                        <Slider
                                            min={0}
                                            max={360}
                                            step={45}
                                            value={form.config.angle}
                                            onChange={(_, d) => setForm(f => ({ ...f, config: { ...f.config, angle: d.value } }))}
                                        />
                                    </Field>
                                )}
                                <Field label={`Размытие (Blur): ${form.config.blur}px`} size="large">
                                    <Slider
                                        min={0}
                                        max={20}
                                        value={form.config.blur}
                                        onChange={(_, d) => setForm(f => ({ ...f, config: { ...f.config, blur: d.value } }))}
                                    />
                                </Field>
                            </div>

                        <Divider />

                        <Subtitle1>Параметры узора</Subtitle1>
                        <div className="flex flex-col gap-4">
                            <Field label="Тип узора" size="large">
                                <Dropdown
                                    size="large"
                                    value={form.config.pattern === "dots" ? "Точки" : form.config.pattern === "lines" ? "Линии" : form.config.pattern === "noise" ? "Шум" : "Нет"}
                                    selectedOptions={[form.config.pattern || "none"]}
                                    onOptionSelect={(_, d) => setForm(f => ({
                                        ...f,
                                        config: { ...f.config, pattern: d.optionValue as any }
                                    }))}
                                >
                                    <Option value="none" text="Нет">Нет</Option>
                                    <Option value="dots" text="Точки">Точки</Option>
                                    <Option value="lines" text="Линии">Линии</Option>
                                    <Option value="noise" text="Шум">Шум</Option>
                                </Dropdown>
                            </Field>
                            {form.config.pattern && form.config.pattern !== "none" && form.config.pattern !== "noise" && (
                                <Field label="Цвет узора" size="large">
                                    <ColorPicker
                                        value={form.config.patternColor || "#ffffff"}
                                        onChange={(val) => setForm(f => ({ ...f, config: { ...f.config, patternColor: val } }))}
                                    />
                                </Field>
                            )}
                        </div>

                        {form.config.pattern && form.config.pattern !== "none" && (
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <Field size="large" label={`Масштаб: ${form.config.patternScale?.toFixed(1) || "1.0"}`}>
                                    <Slider
                                        min={0.1}
                                        max={5}
                                        step={0.1}
                                        value={form.config.patternScale || 1}
                                        onChange={(_, d) => setForm(f => ({ ...f, config: { ...f.config, patternScale: d.value } }))}
                                    />
                                </Field>
                                <Field size="large" label={`Угол: ${form.config.patternAngle || 0}°`}>
                                    <Slider
                                        min={-180}
                                        max={180}

                                        value={form.config.patternAngle || 0}
                                        onChange={(_, d) => setForm(f => ({ ...f, config: { ...f.config, patternAngle: d.value } }))}
                                    />
                                </Field>
                                <Field size="large" label={`Наклон X: ${form.config.patternSkewX || 0}°`}>
                                    <Slider
                                        min={-45}
                                        max={45}
                                        value={form.config.patternSkewX || 0}
                                        onChange={(_, d) => setForm(f => ({ ...f, config: { ...f.config, patternSkewX: d.value } }))}
                                    />
                                </Field>
                                <Field size="large" label={`Наклон Y: ${form.config.patternSkewY || 0}°`}>
                                    <Slider
                                        min={-45}
                                        max={45}
                                        value={form.config.patternSkewY || 0}
                                        onChange={(_, d) => setForm(f => ({ ...f, config: { ...f.config, patternSkewY: d.value } }))}
                                    />
                                </Field>
                                <Field size="large" label={`Смещение X: ${form.config.patternTranslateX || 0}px`}>
                                    <Slider
                                        min={-100}
                                        max={100}
                                        value={form.config.patternTranslateX || 0}
                                        onChange={(_, d) => setForm(f => ({ ...f, config: { ...f.config, patternTranslateX: d.value } }))}
                                    />
                                </Field>
                                <Field size="large" label={`Смещение Y: ${form.config.patternTranslateY || 0}px`}>
                                    <Slider
                                        min={-100}
                                        max={100}
                                        value={form.config.patternTranslateY || 0}
                                        onChange={(_, d) => setForm(f => ({ ...f, config: { ...f.config, patternTranslateY: d.value } }))}
                                    />
                                </Field>
                                <Field size="large" label={`Прозрачность: ${((form.config.patternOpacity || 0.5) * 100).toFixed(0)}%`} className="col-span-2">
                                    <Slider
                                        min={0}
                                        max={1}
                                        step={0.05}
                                        value={form.config.patternOpacity || 0.5}
                                        onChange={(_, d) => setForm(f => ({ ...f, config: { ...f.config, patternOpacity: d.value } }))}
                                    />
                                </Field>
                            </div>
                        )}
                    </div>
                </div>

                <div className="flex flex-col gap-4 flex-1  pl-2 pr-2" style={{ backgroundColor: "var(--colorNeutralBackground2)" }}>
                    <div className="flex flex-col">
                        <Body2  className="uppercase text-slate-400">Предпросмотр</Body2>
                        <Body2 className="text-slate-500">Живой результат</Body2>
                    </div>

                    <div className="flex flex-col gap-4 w-full h-full justify-center items-center content-center">
                        <ThemePreviewCard
                            config={deferredConfig}
                            emoji={form.emoji}
                            name={form.name}
                            textColor={deferredConfig.textColor}
                            className="w-[420px]"
                        />

                        <Card className="w-[420px]">
                            <Subtitle1 block className="mb-2 font-bold">Совет по дизайну</Subtitle1>
                            <Caption1 className="text-slate-600 leading-relaxed italic block">
                                Используйте 2-3 гармоничных цвета для градиента. Мелкие узоры (dots) с низкой прозрачностью (10-20%) придают карточке премиальный вид.
                            </Caption1>
                        </Card>
                    </div>
                </div>
            </div>
        </div>
    );
};
