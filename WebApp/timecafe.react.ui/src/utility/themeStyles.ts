import type { CSSProperties } from "react";

export interface PatternLayer {
    type: "none" | "dots" | "lines" | "noise" | "custom-svg";
    customSvg?: string;
    repeat?: boolean;
    originalColor?: boolean;
    color?: string;
    scale?: number;
    angle?: number;
    skewX?: number;
    skewY?: number;
    translateX?: number;
    translateY?: number;
    opacity?: number;
}

export interface ThemeConfig {
    type: "solid" | "gradient" | "mesh";
    colors: string[];
    angle?: number;
    textColor: string;
    blur?: number;
    patterns?: PatternLayer[];
    pattern?: string;
    patternColor?: string;
    patternScale?: number;
    patternAngle?: number;
    patternSkewX?: number;
    patternSkewY?: number;
    patternTranslateX?: number;
    patternTranslateY?: number;
    patternOpacity?: number;
}

const defaultTheme: ThemeConfig = {
    type: "gradient",
    colors: ["#1a1a2e", "#16213e"],
    angle: 135,
    textColor: "#ffffff",
    blur: 0,
    pattern: "none",
    patternColor: "rgba(255,255,255,0.2)",
    patternScale: 1,
    patternAngle: 0,
    patternSkewX: 0,
    patternSkewY: 0,
    patternTranslateX: 0,
    patternTranslateY: 0,
    patternOpacity: 0.5,
};

export const parseThemeConfig = (configStr?: string | null): ThemeConfig => {
    if (!configStr) return defaultTheme;
    try {
        if (configStr.startsWith("{")) {
            const config = JSON.parse(configStr) as ThemeConfig;

            if (config.pattern && config.pattern !== "none" && !config.patterns) {
                const legacyLayer: PatternLayer = {
                    type: config.pattern as any,
                    color: config.patternColor,
                    customSvg: (config as any).customSvg,
                    repeat: (config as any).patternRepeat,
                    originalColor: (config as any).patternOriginalColor,
                    scale: (config as any).patternScale,
                    angle: (config as any).patternAngle,
                    skewX: (config as any).patternSkewX,
                    skewY: (config as any).patternSkewY,
                    translateX: (config as any).patternTranslateX,
                    translateY: (config as any).patternTranslateY,
                    opacity: (config as any).patternOpacity,
                };
                config.patterns = [legacyLayer];
            }

            return { ...defaultTheme, ...config };
        }
        const colors = configStr.split(",").map(c => c.trim());
        return { ...defaultTheme, type: "gradient", colors };
    } catch {
        return defaultTheme;
    }
};

export const getThemeStyles = (config: ThemeConfig): CSSProperties => {
    const styles: CSSProperties = {
        color: config.textColor,
        position: "relative",
        overflow: "hidden",
    };

    if (config.type === "solid") {
        styles.backgroundColor = config.colors[0];
    } else if (config.type === "gradient") {
        const angle = config.angle ?? 135;
        styles.backgroundImage = `linear-gradient(${angle}deg, ${config.colors.join(", ")})`;
    } else if (config.type === "mesh") {
        styles.backgroundColor = config.colors[0];
        styles.backgroundImage = `
            radial-gradient(at 0% 0%, ${config.colors[1]} 0px, transparent 50%),
            radial-gradient(at 100% 0%, ${config.colors[2] || config.colors[0]} 0px, transparent 50%),
            radial-gradient(at 100% 100%, ${config.colors[3] || config.colors[1]} 0px, transparent 50%),
            radial-gradient(at 0% 100%, ${config.colors[4] || config.colors[0]} 0px, transparent 50%)
        `;
    }

    if (config.blur && config.blur > 0) {
        styles.backdropFilter = `blur(${config.blur}px)`;
        styles.WebkitBackdropFilter = `blur(${config.blur}px)`;
    }

    return styles;
};

export const getPatternLayerStyles = (layer: PatternLayer): CSSProperties => {
    if (!layer.type || layer.type === "none") return { display: "none" };

    const styles: CSSProperties = {
        position: "absolute",
        pointerEvents: "none",
        zIndex: 0,
        opacity: layer.opacity ?? 0.5,
        transform: `
            translate(${layer.translateX ?? 0}px, ${layer.translateY ?? 0}px)
            rotate(${layer.angle ?? 0}deg)
            skew(${layer.skewX ?? 0}deg, ${layer.skewY ?? 0}deg)
            scale(${layer.scale ?? 1})
        `,
        transformOrigin: "center center",
    };

    const isRepeat = layer.repeat ?? false;
    if (isRepeat) {
        styles.inset = "-100%";
    } else {
        styles.inset = "0";
    }

    const pColor = layer.color || "rgba(255,255,255,0.2)";

    if (layer.type === "dots") {
        styles.backgroundImage = `radial-gradient(${pColor} 1.5px, transparent 1.5px)`;
        styles.backgroundSize = "24px 24px";
    } else if (layer.type === "lines") {
        styles.backgroundImage = `linear-gradient(45deg, ${pColor} 25%, transparent 25%, transparent 50%, ${pColor} 50%, ${pColor} 75%, transparent 75%, transparent)`;
        styles.backgroundSize = "16px 16px";
    } else if (layer.type === "noise") {
        styles.backgroundImage = `url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)' opacity='0.1'/%3E%3C/svg%3E")`;
    } else if (layer.type === "custom-svg" && layer.customSvg) {
        const encodedSvg = encodeURIComponent(layer.customSvg);
        const dataUri = `url("data:image/svg+xml,${encodedSvg}")`;

        if (layer.originalColor) {
            styles.backgroundImage = dataUri;
            styles.backgroundColor = "transparent";
        } else {
            styles.backgroundColor = pColor;
            styles.WebkitMaskImage = dataUri;
            styles.maskImage = dataUri;
        }

        if (isRepeat) {
            styles.WebkitMaskRepeat = "repeat";
            styles.maskRepeat = "repeat";
            styles.backgroundRepeat = "repeat";
            styles.WebkitMaskSize = "40px 40px";
            styles.maskSize = "40px 40px";
            styles.backgroundSize = "40px 40px";
        } else {
            styles.WebkitMaskRepeat = "no-repeat";
            styles.maskRepeat = "no-repeat";
            styles.backgroundRepeat = "no-repeat";
            styles.WebkitMaskPosition = "center";
            styles.maskPosition = "center";
            styles.backgroundPosition = "center";
            styles.WebkitMaskSize = "contain";
            styles.maskSize = "contain";
            styles.backgroundSize = "contain";
        }
    }

    return styles;
};

export const getPatternStyles = (config: ThemeConfig): CSSProperties => {
    if (config.patterns && config.patterns.length > 0) {
        return getPatternLayerStyles(config.patterns[0]);
    }
    return { display: "none" };
};
