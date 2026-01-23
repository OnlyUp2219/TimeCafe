import {Caption1, Card, ProgressBar, Title2, Title3} from "@fluentui/react-components";

import {Gift20Regular} from "@fluentui/react-icons";

type LoyaltyCardProps = {
    statusLabel: string;
    progress: number;
    spentRubText: string;
    leftRubText: string;
    perkText: string;
};

export const LoyaltyCard = ({statusLabel, progress, spentRubText, leftRubText, perkText}: LoyaltyCardProps) => {
    return (
        <Card className="flex flex-col gap-4">
            <Title3>Лояльность</Title3>
            <div className="text-center flex flex-col gap-1">
                <Caption1 block>Ваш статус</Caption1>
                <Title2>{statusLabel}</Title2>
            </div>
            <ProgressBar value={progress} color="brand" />
            <div className="flex justify-between">
                <Caption1>{spentRubText}</Caption1>
                <Caption1>{leftRubText}</Caption1>
            </div>
            <Card appearance="subtle">
                <div className="flex gap-2">
                    <Gift20Regular />
                    <Caption1>{perkText}</Caption1>
                </div>
            </Card>
        </Card>
    );
};
