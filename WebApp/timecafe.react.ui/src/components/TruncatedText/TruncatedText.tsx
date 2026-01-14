import {Text, Tooltip} from "@fluentui/react-components";
import type {CSSProperties, FC, ReactNode} from "react";
import {useLayoutEffect, useMemo, useRef, useState} from "react";

type TruncatedTextProps = {
    children: string;
    lines?: number;
    className?: string;
    textStyle?: CSSProperties;
    size?: 200 | 300 | 400 | 500 | 600 | 700 | 800 | 900 | 1000;
    weight?: "regular" | "medium" | "semibold" | "bold";
};

export const TruncatedText: FC<TruncatedTextProps> = ({
    children,
    lines = 2,
    className,
    textStyle,
    size,
    weight,
}) => {
    const containerRef = useRef<HTMLDivElement | null>(null);
    const [isOverflowing, setIsOverflowing] = useState(false);

    const clampStyle = useMemo((): React.CSSProperties => {
        return {
            overflow: "hidden",
            display: "-webkit-box",
            WebkitBoxOrient: "vertical",
            WebkitLineClamp: lines,
        };
    }, [lines]);

    useLayoutEffect(() => {
        const el = containerRef.current;
        if (!el) return;

        const check = () => {
            const next = el.scrollHeight > el.clientHeight + 1;
            setIsOverflowing(next);
        };

        check();

        if (typeof ResizeObserver === "undefined") return;

        const ro = new ResizeObserver(() => {
            check();
        });
        ro.observe(el);

        return () => {
            ro.disconnect();
        };
    }, [children, lines]);

    const content: ReactNode = (
        <div ref={containerRef} className={className} style={clampStyle}>
            <Text block style={textStyle} size={size} weight={weight}>
                {children}
            </Text>
        </div>
    );

    if (!isOverflowing) return <>{content}</>;

    return (
        <Tooltip content={children} relationship="description">
            {content}
        </Tooltip>
    );
};
