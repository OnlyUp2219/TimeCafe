import {Caption1, Card, Subtitle2Stronger, Title2} from "@fluentui/react-components";

import {formatDurationMinutes} from "@utility/formatDurationMinutes";
import {formatRub} from "@utility/formatRub";

import "@pages/billing/billing.css";

type RestTimeCardProps = {
    availableRub: number;
    tariffName: string;
    pricePerMinuteRub: number;
};

export const RestTimeCard = ({availableRub, tariffName, pricePerMinuteRub}: RestTimeCardProps) => {
    const minutes = pricePerMinuteRub > 0 ? Math.floor(Math.max(0, availableRub) / pricePerMinuteRub) : 0;

    return (
        <Card className="flex h-full flex-col justify-between gap-2 tc-billing-rest-card">
            <Caption1 block className="opacity-80">
                Хватит на отдых
            </Caption1>
            <Title2 block>
                ~ {formatDurationMinutes(minutes)}
            </Title2>

            <div className="flex flex-col gap-1">
                <Caption1 block>Расчёт по тарифу:</Caption1>
                <Subtitle2Stronger block>
                    {tariffName} ({formatRub(pricePerMinuteRub, 0)} / мин)
                </Subtitle2Stronger>
            </div>
        </Card>
    );
};
