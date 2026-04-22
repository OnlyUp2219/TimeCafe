import type { CSSProperties } from "react";

export interface ThemeConfig {
    type: "solid" | "gradient" | "mesh";
    colors: string[];
    angle?: number;
    textColor: string;
    blur?: number;
    pattern?: "none" | "dots" | "lines" | "noise";
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
            return { ...defaultTheme, ...JSON.parse(configStr) };
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

export const getPatternStyles = (config: ThemeConfig): CSSProperties => {
    if (!config.pattern || config.pattern === "none") return { display: "none" };

    const styles: CSSProperties = {
        position: "absolute",
        inset: "-100%", // Extra space for rotations/skews
        pointerEvents: "none",
        zIndex: 0,
        opacity: config.patternOpacity ?? 0.5,
        transform: `
            translate(${config.patternTranslateX ?? 0}px, ${config.patternTranslateY ?? 0}px)
            rotate(${config.patternAngle ?? 0}deg)
            skew(${config.patternSkewX ?? 0}deg, ${config.patternSkewY ?? 0}deg)
            scale(${config.patternScale ?? 1})
        `,
        transformOrigin: "center center",
    };

    const pColor = config.patternColor || "rgba(255,255,255,0.2)";
    
    if (config.pattern === "dots") {
        styles.backgroundImage = `radial-gradient(${pColor} 1.5px, transparent 1.5px)`;
        styles.backgroundSize = "24px 24px";
    } else if (config.pattern === "lines") {
        styles.backgroundImage = `linear-gradient(45deg, ${pColor} 25%, transparent 25%, transparent 50%, ${pColor} 50%, ${pColor} 75%, transparent 75%, transparent)`;
        styles.backgroundSize = "16px 16px";
    } else if (config.pattern === "noise") {
        styles.backgroundImage = `url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)' opacity='0.1'/%3E%3C/svg%3E")`;
    }

    return styles;
};
