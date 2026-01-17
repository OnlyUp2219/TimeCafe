import {Body2, Text, Title3, Tooltip} from "@fluentui/react-components";
import type {CSSProperties, FC, ReactNode} from "react";
import {useLayoutEffect, useMemo, useRef, useState} from "react";

type TruncatedTextProps = {
    children: string;
    lines?: number;
    className?: string;
    textStyle?: CSSProperties;
    as?: "body2" | "text" | "title3";
};

export const TruncatedText: FC<TruncatedTextProps> = ({
    children,
    lines = 2,
    className,
    textStyle,
    as = "body2",
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

    const textNode = useMemo((): ReactNode => {
        if (as === "title3") return <Title3 block style={textStyle}>{children}</Title3>;
        if (as === "text") return <Text block style={textStyle}>{children}</Text>;
        return <Body2 block style={textStyle}>{children}</Body2>;
    }, [as, children, textStyle]);

    const content: ReactNode = (
        <div ref={containerRef} className={className} style={clampStyle}>
            {textNode}
        </div>
    );

    if (!isOverflowing) return <>{content}</>;

    return (
        <Tooltip content={children} relationship="description">
            {content}
        </Tooltip>
    );
};
