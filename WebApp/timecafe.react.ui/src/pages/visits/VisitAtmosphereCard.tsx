import type {FC} from "react";
import {Body1, Divider, ProgressBar, Subtitle2Stronger} from "@fluentui/react-components";
import {WeatherMoon20Regular} from "@fluentui/react-icons";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import {BillingType as BillingTypeEnum, type BillingType} from "@app-types/tariff";

interface VisitAtmosphereCardProps {
    billingType: BillingType;
    progressToNextHour: number | null;
    elapsedMinutes: number;
}

export const VisitAtmosphereCard: FC<VisitAtmosphereCardProps> = ({
    billingType,
    progressToNextHour,
    elapsedMinutes,
}) => (
    <HoverTiltCard className="!hidden lg:!block lg:col-span-4">
        <div className="flex flex-col gap-4">
            <div className="flex items-center gap-2">
                <WeatherMoon20Regular/>
                <Subtitle2Stronger>Атмосфера</Subtitle2Stronger>
            </div>

            <Body1 block>
                Небольшая панель для полезной информации: правила, Wi‑Fi, розетки, кухня и
                быстрые подсказки по тарифу.
            </Body1>

            <Divider/>

            {billingType === BillingTypeEnum.Hourly && progressToNextHour !== null ? (
                <div className="flex flex-col gap-2">
                    <Body1 block>Прогресс до следующего часа</Body1>
                    <ProgressBar value={progressToNextHour}/>
                    <Body1 block>
                        {elapsedMinutes % 60} мин из 60
                    </Body1>
                </div>
            ) : (
                <div className="flex flex-col gap-2">
                    <Body1 block>Статус визита</Body1>
                    <ProgressBar/>
                    <Body1 block>Активен — обновляется в реальном времени</Body1>
                </div>
            )}
        </div>
    </HoverTiltCard>
);
