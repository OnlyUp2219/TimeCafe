import type { FC } from "react";
import { Button, Title2, Body2, Card } from "@fluentui/react-components";
import { Clock48Regular, Dismiss20Regular } from "@fluentui/react-icons";

interface PendingVisitStateProps {
    sizes: any;
    cancellingVisit: boolean;
    onCancelClick: () => void;
}

export const PendingVisitState: FC<PendingVisitStateProps> = ({
    sizes,
    cancellingVisit,
    onCancelClick,
}) => {
    return (
        <Card size={sizes.card} className="flex flex-col gap-4 items-center text-center">
            <Clock48Regular />
            <Title2>Ожидаем подтверждения</Title2>
            <Body2>Менеджер скоро рассмотрит вашу заявку на визит.</Body2>
            <div className="flex gap-2 flex-wrap justify-center">
                <Button
                    appearance="primary"
                    size={sizes.button}
                    onClick={onCancelClick}
                    disabled={cancellingVisit}
                    icon={<Dismiss20Regular />}
                >
                    Отменить заявку
                </Button>
            </div>
        </Card>
    );
};
