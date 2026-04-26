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
    Textarea,
    Switch,
    TabList,
    Tab,
    Overflow,
    OverflowItem,
    useOverflowMenu,
    useIsOverflowItemVisible,
    Menu,
    MenuTrigger,
    MenuPopover,
    MenuList,
    MenuItem,
    MenuButton,
} from "@fluentui/react-components";
import { ArrowLeft20Regular, Color20Regular, Add20Regular, Delete20Regular, MoreHorizontal20Regular } from "@fluentui/react-icons";
import { tinycolor } from "@ctrl/tinycolor";
import {
    useGetAllThemesQuery,
    useCreateThemeMutation,
    useUpdateThemeMutation,
} from "@store/api/venueApi";
import { getRtkErrorMessage } from "@shared/api/errors/extractRtkError";
import type { FetchBaseQueryError } from "@reduxjs/toolkit/query";
import { EmojiPicker } from "@components/EmojiPicker/EmojiPicker";
import { parseThemeConfig, getThemeStyles, getPatternLayerStyles, type ThemeConfig, type PatternLayer } from "@utility/themeStyles";

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
                                size="large"
                                value={displayValue}
                                onChange={(_, d) => onChange(d.value)}
                                className="grow font-mono uppercase text-lg"
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

    return (
        <Card
            className={className}
            style={{
                ...styles,
                minHeight: "240px",
            }}
        >
            <div className="absolute inset-0 bg-black/10 z-0" />
            {(config.patterns || []).map((layer, idx) => (
                <div key={idx} style={getPatternLayerStyles(layer)} />
            ))}
            <CardHeader
                className="relative z-10"
                image={<span style={{ fontSize: "36px" }}>{emoji}</span>}
                header={<Title2 style={{ color: textColor }}>{name || "Название тарифа"}</Title2>}
                description={
                    <Body2 style={{ color: textColor, opacity: 0.8 }} size={400}>
                        Здесь будет описание вашего тарифа. Используйте яркие темы для привлечения внимания гостей!
                    </Body2>
                }
            />

            <Divider style={{ color: textColor, opacity: 0.3 }} className="relative z-10" />

            <div className="flex flex-col gap-1 px-3 relative z-10">
                <Caption1 style={{ color: textColor, opacity: 0.7 }} size={300}>Стандартный</Caption1>
                <Title2 style={{ color: textColor }}>15.00 ₽ / мин</Title2>
            </div>

            <CardFooter className="relative z-10">
                <Button 
                    appearance="primary" 
                    style={{ backgroundColor: "rgba(255,255,255,0.2)", color: textColor }}
                >
                    Выбрать тариф
                </Button>
            </CardFooter>
        </Card>
    );
});


const OverflowMenu = ({ onTabSelect, patterns }: { onTabSelect: (id: string) => void; patterns: PatternLayer[] }) => {
    const { ref, isOverflowing, overflowCount } = useOverflowMenu<HTMLButtonElement>();

    if (!isOverflowing) return null;

    return (
        <Menu hasIcons>
            <MenuTrigger disableButtonEnhancement>
                <Button
                    ref={ref}
                    appearance="transparent"
                    role="tab"
                    size="large"
                    icon={<MoreHorizontal20Regular />}
                >
                    +{overflowCount}
                </Button>
            </MenuTrigger>
            <MenuPopover>
                <MenuList>
                    {patterns.map((layer, i) => (
                        <OverflowMenuItem key={i} id={i.toString()} name={`Слой #${i + 1}`} onClick={() => onTabSelect(i.toString())} layer={layer} />
                    ))}
                </MenuList>
            </MenuPopover>
        </Menu>
    );
};

const OverflowMenuItem = ({ id, name, onClick, layer }: { id: string; name: string; onClick: () => void; layer: PatternLayer }) => {
    const isVisible = useIsOverflowItemVisible(id);
    if (isVisible) return null;
    return <MenuItem onClick={onClick} icon={<MiniPreview layer={layer} />}>{name}</MenuItem>;
};

const emptyConfig: ThemeConfig = {
    type: "gradient",
    colors: ["#1a1a2e", "#16213e"],
    angle: 135,
    textColor: "#ffffff",
    blur: 0,
    patterns: [],
};

const LayerPreview = memo(({ layer }: { layer: PatternLayer }) => {
    const styles = useMemo(() => getPatternLayerStyles(layer), [layer]);
    return (
        <div className="relative w-full h-32 rounded-lg border-2 border-dashed border-gray-300 overflow-hidden bg-slate-50 flex items-center justify-center">
            <div className="absolute inset-0 opacity-10 bg-slate-900" style={{ backgroundImage: 'radial-gradient(#000 10%, transparent 10%)', backgroundSize: '10px 10px' }} />
            <div style={{ ...styles, inset: 0 }} />
            <Caption1 className="absolute bottom-1 right-2 text-gray-400">Предпросмотр слоя</Caption1>
        </div>
    );
});

const MiniPreview = memo(({ layer }: { layer: PatternLayer }) => {
    const styles = useMemo(() => getPatternLayerStyles(layer), [layer]);
    return (
        <div className="w-5 h-5 rounded border border-gray-200 relative overflow-hidden bg-slate-100">
            <div style={{ ...styles, inset: 0, transform: (styles.transform || '') + ' scale(0.5)' }} />
        </div>
    );
});

export const ThemeEditorPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [selectedTab, setSelectedTab] = useState<string>("0");
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

    if (id && loadingThemes) return <Spinner size="huge" label="Загрузка темы..." />;

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
            <Divider />
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
                        <div className="flex flex-wrap gap-4">
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
                            <Field label="Цвета палитры" size="large">
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

                        <Subtitle1>Слои узоров</Subtitle1>
                        <div className="flex flex-col gap-4">
                            {(form.config.patterns || []).length > 0 && (
                                <Overflow padding={40}>
                                    <div className="flex flex-nowrap min-w-0 overflow-hidden items-center">
                                        <TabList
                                            size="large"
                                            selectedValue={selectedTab}
                                            onTabSelect={(_, d) => setSelectedTab(d.value as string)}
                                        >
                                            {(form.config.patterns || []).map((layer, idx) => (
                                                <OverflowItem key={idx} id={idx.toString()}>
                                                    <Tab
                                                        value={idx.toString()}
                                                    >
                                                        Слой #{idx + 1}
                                                    </Tab>
                                                </OverflowItem>
                                            ))}
                                        </TabList>
                                        <OverflowMenu patterns={form.config.patterns || []} onTabSelect={setSelectedTab} />
                                    </div>
                                </Overflow>
                            )}

                            {(form.config.patterns || []).map((layer, idx) => (
                                idx.toString() === selectedTab && (
                                    <Card key={idx} appearance="filled-alternative" >
                                        <div className="absolute top-2 right-2 flex gap-2">
                                            <Popover>
                                                <PopoverTrigger disableButtonEnhancement>
                                                    <Button size="large" appearance="subtle" icon={<Delete20Regular className="text-red-500" />} />
                                                </PopoverTrigger>
                                                <PopoverSurface>
                                                    <div className="flex flex-col gap-2">
                                                        <Body2>Удалить этот слой?</Body2>
                                                        <div className="flex gap-2">
                                                            <Button
                                                                appearance="primary"
                                                                onClick={() => {
                                                                    const newPatterns = form.config.patterns?.filter((_, i) => i !== idx) || [];
                                                                    setForm(f => ({
                                                                        ...f,
                                                                        config: { ...f.config, patterns: newPatterns }
                                                                    }));
                                                                    setSelectedTab("0");
                                                                }}
                                                            >
                                                                Да, удалить
                                                            </Button>
                                                        </div>
                                                    </div>
                                                </PopoverSurface>
                                            </Popover>
                                        </div>

                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                            <Field label="Тип узора" size="large">
                                                <Dropdown
                                                    size="large"
                                                    value={layer.type === "dots" ? "Точки" : layer.type === "lines" ? "Линии" : layer.type === "noise" ? "Шум" : layer.type === "custom-svg" ? "Своя SVG" : "Нет"}
                                                    selectedOptions={[layer.type]}
                                                    onOptionSelect={(_, d) => {
                                                        const newPatterns = [...(form.config.patterns || [])];
                                                        newPatterns[idx] = { ...newPatterns[idx], type: d.optionValue as any };
                                                        setForm(f => ({ ...f, config: { ...f.config, patterns: newPatterns } }));
                                                    }}
                                                >
                                                    <Option value="none">Нет</Option>
                                                    <Option value="dots">Точки</Option>
                                                    <Option value="lines">Линии</Option>
                                                    <Option value="noise">Шум</Option>
                                                    <Option value="custom-svg">Своя SVG</Option>
                                                </Dropdown>
                                            </Field>

                                            {layer.type === "custom-svg" && (
                                                <Field 
                                                    label="Ваш SVG (код)" 
                                                    className="col-span-2" 
                                                    size="large"
                                                    validationState={layer.customSvg && !layer.customSvg.trim().toLowerCase().startsWith("<svg") ? "error" : "none"}
                                                    validationMessage={layer.customSvg && !layer.customSvg.trim().toLowerCase().startsWith("<svg") ? "Код должен начинаться с <svg" : undefined}
                                                >
                                                    <Textarea
                                                        size="large"
                                                        value={layer.customSvg || ""}
                                                        onChange={(_, d) => {
                                                            const newPatterns = [...(form.config.patterns || [])];
                                                            newPatterns[idx] = { ...newPatterns[idx], customSvg: d.value };
                                                            setForm(f => ({ ...f, config: { ...f.config, patterns: newPatterns } }));
                                                        }}
                                                        resize="vertical"
                                                        textarea={{ className: "font-mono text-xs h-32" }}
                                                    />
                                                </Field>
                                            )}

                                            <div className="flex gap-6 flex-wrap col-span-2">
                                                {layer.type === "custom-svg" && (
                                                    <>
                                                        <Switch
                                                            label="Зациклить"
                                                            checked={layer.repeat ?? false}
                                                            onChange={(_, d) => {
                                                                const newPatterns = [...(form.config.patterns || [])];
                                                                newPatterns[idx] = { ...newPatterns[idx], repeat: d.checked };
                                                                setForm(f => ({ ...f, config: { ...f.config, patterns: newPatterns } }));
                                                            }}
                                                        />
                                                        <Switch
                                                            label="Ориг. цвет"
                                                            checked={layer.originalColor ?? false}
                                                            onChange={(_, d) => {
                                                                const newPatterns = [...(form.config.patterns || [])];
                                                                newPatterns[idx] = { ...newPatterns[idx], originalColor: d.checked };
                                                                setForm(f => ({ ...f, config: { ...f.config, patterns: newPatterns } }));
                                                            }}
                                                        />
                                                    </>
                                                )}
                                                {layer.type !== "none" && layer.type !== "noise" && !layer.originalColor && (
                                                    <Field label="Цвет" size="large">
                                                        <ColorPicker
                                                            value={layer.color || "#ffffff33"}
                                                            onChange={(val) => {
                                                                const newPatterns = [...(form.config.patterns || [])];
                                                                newPatterns[idx] = { ...newPatterns[idx], color: val };
                                                                setForm(f => ({ ...f, config: { ...f.config, patterns: newPatterns } }));
                                                            }}
                                                        />
                                                    </Field>
                                                )}
                                            </div>

                                            <Field label={`Масштаб: ${layer.scale?.toFixed(1) || "1.0"}`} size="large">
                                                <Slider
                                                    min={0.1} max={10} step={0.1}
                                                    value={layer.scale || 1}
                                                    onChange={(_, d) => {
                                                        const newPatterns = [...(form.config.patterns || [])];
                                                        newPatterns[idx] = { ...newPatterns[idx], scale: d.value };
                                                        setForm(f => ({ ...f, config: { ...f.config, patterns: newPatterns } }));
                                                    }}
                                                />
                                            </Field>
                                            <Field label={`Угол: ${layer.angle || 0}°`} size="large">
                                                <Slider
                                                    min={-180} max={180}
                                                    value={layer.angle || 0}
                                                    onChange={(_, d) => {
                                                        const newPatterns = [...(form.config.patterns || [])];
                                                        newPatterns[idx] = { ...newPatterns[idx], angle: d.value };
                                                        setForm(f => ({ ...f, config: { ...f.config, patterns: newPatterns } }));
                                                    }}
                                                />
                                            </Field>
                                            <Field label={`Смещение X: ${layer.translateX || 0}px`} size="large">
                                                <Slider
                                                    min={-200} max={200}
                                                    value={layer.translateX || 0}
                                                    onChange={(_, d) => {
                                                        const newPatterns = [...(form.config.patterns || [])];
                                                        newPatterns[idx] = { ...newPatterns[idx], translateX: d.value };
                                                        setForm(f => ({ ...f, config: { ...f.config, patterns: newPatterns } }));
                                                    }}
                                                />
                                            </Field>
                                            <Field label={`Смещение Y: ${layer.translateY || 0}px`} size="large">
                                                <Slider
                                                    min={-200} max={200}
                                                    value={layer.translateY || 0}
                                                    onChange={(_, d) => {
                                                        const newPatterns = [...(form.config.patterns || [])];
                                                        newPatterns[idx] = { ...newPatterns[idx], translateY: d.value };
                                                        setForm(f => ({ ...f, config: { ...f.config, patterns: newPatterns } }));
                                                    }}
                                                />
                                            </Field>
                                            <Field label={`Прозрачность: ${((layer.opacity ?? 0.5) * 100).toFixed(0)}%`} className="col-span-2" size="large">
                                                <Slider
                                                    min={0} max={1} step={0.05}
                                                    value={layer.opacity ?? 0.5}
                                                    onChange={(_, d) => {
                                                        const newPatterns = [...(form.config.patterns || [])];
                                                        newPatterns[idx] = { ...newPatterns[idx], opacity: d.value };
                                                        setForm(f => ({ ...f, config: { ...f.config, patterns: newPatterns } }));
                                                    }}
                                                />
                                            </Field>
                                            <div className="col-span-1 md:col-span-2">
                                                <LayerPreview layer={layer} />
                                            </div>
                                        </div>
                                    </Card>
                                )
                            ))}

                            <Button
                                appearance="outline"
                                size="large"
                                icon={<Add20Regular />}
                                disabled={(form.config.patterns || []).length >= 20}
                                onClick={() => {
                                    const patterns = form.config.patterns || [];
                                    if (patterns.length >= 20) return;
                                    const nextIdx = patterns.length;
                                    setForm(f => ({
                                        ...f,
                                        config: {
                                            ...f.config,
                                            patterns: [...patterns, { type: "dots", scale: 1, opacity: 0.2, color: "#ffffff33" }]
                                        }
                                    }));
                                    setSelectedTab(nextIdx.toString());
                                }}
                            >
                                Добавить слой узора
                            </Button>
                        </div>
                    </div>
                </div>

                <div className="flex flex-col gap-4 flex-1  pl-2 pr-2" style={{ backgroundColor: "var(--colorNeutralBackground2)" }}>
                    <div className="flex flex-col">
                        <Body2 className="uppercase text-slate-400">Предпросмотр</Body2>
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
