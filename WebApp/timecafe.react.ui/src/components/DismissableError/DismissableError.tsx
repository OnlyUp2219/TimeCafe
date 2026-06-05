import { useState, useEffect } from "react";
import { MessageBar, MessageBarBody, MessageBarActions, Button } from "@fluentui/react-components";
import { DismissRegular } from "@fluentui/react-icons";

interface DismissableErrorProps {
    error: string | null | undefined;
    className?: string;
    onDismiss?: () => void;
}

export const DismissableError = ({ error, className, onDismiss }: DismissableErrorProps) => {
    const [dismissed, setDismissed] = useState(false);

    useEffect(() => {
        if (error) {
            setDismissed(false);
        }
    }, [error]);

    if (!error || dismissed) {
        return null;
    }

    const handleDismiss = () => {
        setDismissed(true);
        if (onDismiss) {
            onDismiss();
        }
    };

    return (
        <MessageBar intent="error" className={className}>
            <MessageBarBody>{error}</MessageBarBody>
            <MessageBarActions
                containerAction={
                    <Button
                        onClick={handleDismiss}
                        appearance="transparent"
                        aria-label="Dismiss"
                        icon={<DismissRegular />}
                    />
                }
            />
        </MessageBar>
    );
};
