import {createContext, useContext, useMemo, useState, type FC, type ReactNode} from "react";

type FluentSize = "small" | "medium" | "large";

interface ComponentSizeMap {
    input: FluentSize;
    button: FluentSize;
    field: FluentSize;
    badge: "small" | "medium" | "large" | "extra-large";
    card: "small" | "medium" | "large";
    dropdown: FluentSize;
    combobox: FluentSize;
    checkbox: FluentSize;
    radiogroup: FluentSize;
    avatar: 16 | 20 | 24 | 28 | 32 | 36 | 40 | 48 | 56 | 64 | 72 | 96 | 120 | 128;
    spinner: "tiny" | "extra-small" | "small" | "medium" | "large" | "extra-large" | "huge";
}

const SIZE_PRESETS: Record<FluentSize, ComponentSizeMap> = {
    large: {
        input: "large",
        button: "large",
        field: "large",
        badge: "large",
        card: "large",
        dropdown: "large",
        combobox: "large",
        checkbox: "large",
        radiogroup: "large",
        avatar: 48,
        spinner: "large",
    },
    medium: {
        input: "medium",
        button: "medium",
        field: "medium",
        badge: "medium",
        card: "medium",
        dropdown: "medium",
        combobox: "medium",
        checkbox: "medium",
        radiogroup: "medium",
        avatar: 36,
        spinner: "medium",
    },
    small: {
        input: "small",
        button: "small",
        field: "small",
        badge: "small",
        card: "small",
        dropdown: "small",
        combobox: "small",
        checkbox: "small",
        radiogroup: "small",
        avatar: 28,
        spinner: "small",
    },
};

interface ComponentSizeContextValue {
    sizes: ComponentSizeMap;
    preset: FluentSize;
    setPreset: (preset: FluentSize) => void;
}

const ComponentSizeContext = createContext<ComponentSizeContextValue>({
    sizes: SIZE_PRESETS.large,
    preset: "large",
    setPreset: () => {},
});

export const ComponentSizeProvider: FC<{children: ReactNode; defaultPreset?: FluentSize}> = ({children, defaultPreset = "large"}) => {
    const [preset, setPreset] = useState<FluentSize>(defaultPreset);
    const value = useMemo(() => ({sizes: SIZE_PRESETS[preset], preset, setPreset}), [preset]);
    return <ComponentSizeContext.Provider value={value}>{children}</ComponentSizeContext.Provider>;
};

export const useComponentSize = () => useContext(ComponentSizeContext);
