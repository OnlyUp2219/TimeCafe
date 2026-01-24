import {Button, Text, Tooltip, type ButtonProps} from "@fluentui/react-components";
import type {MouseEventHandler} from "react";

type TooltipButtonProps = {
    tooltip: string;
    label: string;
    icon?: ButtonProps["icon"];

    appearance?: ButtonProps["appearance"];
    size?: ButtonProps["size"];
    disabled?: boolean;
    className?: string;
    onClick?: MouseEventHandler<HTMLButtonElement>;
};

export const TooltipButton = ({tooltip, label, icon, ...buttonProps}: TooltipButtonProps) => {
    const button = (
        <Button as="button" {...buttonProps} icon={icon}>
            <Text truncate wrap={false}>
                {label}
            </Text>
        </Button>
    );

    return (
        <Tooltip content={tooltip} relationship="label">
            {buttonProps.disabled ? <span>{button}</span> : button}
        </Tooltip>
    );
};
