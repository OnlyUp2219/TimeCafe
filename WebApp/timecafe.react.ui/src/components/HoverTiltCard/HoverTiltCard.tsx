import {Card, tokens} from "@fluentui/react-components";
import type {FC, PropsWithChildren} from "react";
import {useEffect, useMemo, useRef, useState} from "react";

type HoverTiltCardProps = PropsWithChildren<{
    className?: string;
    maxTiltDeg?: number;
    hoverScale?: number;
    perspectivePx?: number;
}>;

export const HoverTiltCard: FC<HoverTiltCardProps> = ({
    children,
    className,
    maxTiltDeg = 4,
    hoverScale = 1.04,
    perspectivePx = 900,
}) => {
    const elementRef = useRef<HTMLDivElement | null>(null);
    const rafRef = useRef<number | null>(null);
    const hoverRef = useRef(false);

    const [isHover, setIsHover] = useState(false);
    const [transform, setTransform] = useState<string>("perspective(900px) rotateX(0deg) rotateY(0deg) scale(1)");

    const baseClassName = useMemo(() => {
        const base = "transform-gpu transition-[transform,box-shadow] duration-150 ease-out will-change-transform";
        return className ? `${base} ${className}` : base;
    }, [className]);

    const cancelPendingFrame = () => {
        if (rafRef.current) {
            window.cancelAnimationFrame(rafRef.current);
            rafRef.current = null;
        }
    };

    const reset = () => {
        cancelPendingFrame();
        setTransform(`perspective(${perspectivePx}px) rotateX(0deg) rotateY(0deg) scale(1)`);
    };

    const scheduleUpdate = (clientX: number, clientY: number) => {
        if (!elementRef.current) return;
        if (rafRef.current) return;

        rafRef.current = window.requestAnimationFrame(() => {
            rafRef.current = null;
            if (!hoverRef.current) return;
            const el = elementRef.current;
            if (!el) return;

            const rect = el.getBoundingClientRect();
            const px = (clientX - rect.left) / rect.width;
            const py = (clientY - rect.top) / rect.height;

            const clampedX = Math.max(0, Math.min(1, px));
            const clampedY = Math.max(0, Math.min(1, py));

            const dx = (clampedX - 0.5) * 2;
            const dy = (clampedY - 0.5) * 2;

            const rotateY = dx * maxTiltDeg;
            const rotateX = -dy * maxTiltDeg;

            setTransform(
                `perspective(${perspectivePx}px) rotateX(${rotateX.toFixed(2)}deg) rotateY(${rotateY.toFixed(2)}deg) scale(${hoverScale})`
            );
        });
    };

    useEffect(() => {
        return () => {
            cancelPendingFrame();
        };
    }, []);

    return (
        <Card
            ref={elementRef}
            className={baseClassName}
            style={{
                transform,
                boxShadow: isHover ? tokens.shadow16 : tokens.shadow4,
                backgroundColor: isHover ? tokens.colorNeutralBackground1Hover : tokens.colorNeutralBackground1,
                borderColor: isHover ? tokens.colorBrandStroke1 : tokens.colorNeutralStroke1,
            }}
            onPointerEnter={(e) => {
                if (e.pointerType !== "mouse") return;
                hoverRef.current = true;
                setIsHover(true);
                scheduleUpdate(e.clientX, e.clientY);
            }}
            onPointerMove={(e) => {
                if (e.pointerType !== "mouse") return;
                if (!isHover) return;
                scheduleUpdate(e.clientX, e.clientY);
            }}
            onPointerLeave={() => {
                hoverRef.current = false;
                setIsHover(false);
                reset();
            }}
            onPointerCancel={() => {
                hoverRef.current = false;
                setIsHover(false);
                reset();
            }}
        >
            {children}
        </Card>
    );
};
