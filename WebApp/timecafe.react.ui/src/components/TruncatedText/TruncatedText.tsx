import {Body2, Tooltip} from "@fluentui/react-components";
import type {CSSProperties, FC, ReactNode} from "react";
import {useLayoutEffect, useMemo, useRef, useState} from "react";

type TruncatedTextProps = {
    children: string;
    lines?: number;
    className?: string;
    textStyle?: CSSProperties;
};

export const TruncatedText: FC<TruncatedTextProps> = ({
    children,
    lines = 2,
    className,
    textStyle,
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
            <Body2 block style={textStyle}>{children}</Body2>
        </div>
    );

    if (!isOverflowing) return <>{content}</>;

    return (
        <Tooltip content={children} relationship="description">
            {content}
        </Tooltip>
    );
};
