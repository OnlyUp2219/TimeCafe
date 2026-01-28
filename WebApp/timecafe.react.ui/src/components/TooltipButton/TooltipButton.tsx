import {Button, Text, Tooltip, type ButtonProps, type TooltipProps} from "@fluentui/react-components";
import type {MouseEventHandler} from "react";

type TooltipButtonProps = {
    tooltip: string;
    label: string;
    icon?: ButtonProps["icon"];

    positioning?: TooltipProps["positioning"];

    appearance?: ButtonProps["appearance"];
    size?: ButtonProps["size"];
    disabled?: boolean;
    className?: string;
    type?: "button" | "submit" | "reset";
    onClick?: MouseEventHandler<HTMLButtonElement>;
};

export const TooltipButton = ({tooltip, label, icon, positioning = "below", ...buttonProps}: TooltipButtonProps) => {
    const button = (
        <Button as="button" {...buttonProps} icon={icon}>
            <Text truncate wrap={false}>
                {label}
            </Text>
        </Button>
    );

    return (
        <Tooltip content={tooltip} relationship="label" positioning={positioning}>
            {button}
        </Tooltip>
    );
};
