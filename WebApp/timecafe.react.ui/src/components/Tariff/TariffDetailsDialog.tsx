import type { FC } from "react";
import {
    Body1,
    Body2,
    Button,
    Divider,
    Title2,
    Dialog,
    DialogSurface,
    DialogBody,
    DialogTitle,
    DialogContent,
    Subtitle2,
    Caption1,
} from "@fluentui/react-components";
import { Dismiss20Regular } from "@fluentui/react-icons";
import { BillingType as BillingTypeEnum } from "@app-types/tariff";
import { useGetTariffByIdQuery } from "@store/api/venueApi";
import { CURRENCY_SYMBOL } from "@shared/const/currency";

interface TariffDetailsDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    tariffId: string | null;
}

export const TariffDetailsDialog: FC<TariffDetailsDialogProps> = ({
    open,
    onOpenChange,
    tariffId,
}) => {
    const { data: tariff, isLoading } = useGetTariffByIdQuery(tariffId ?? "", { skip: !tariffId });

    return (
        <Dialog
            open={open}
            onOpenChange={(_, data) => onOpenChange(data.open)}
        >
            <DialogSurface className="overflow-hidden p-0!">
                <div id="profileGateBackground" className="overflow-hidden relative ">
                    <div className="profile-gate-bg" aria-hidden="true">
                        <span className="profile-gate-shape profile-gate-shape--circle" />
                        <span className="profile-gate-shape profile-gate-shape--square" />
                        <span className="profile-gate-shape profile-gate-shape--triangle" />
                    </div>
                    <DialogBody className="z-10 w-full p-6!">
                        <DialogTitle
                            action={
                                <Button
                                    appearance="subtle"
                                    aria-label="Close"
                                    icon={<Dismiss20Regular />}
                                    onClick={() => onOpenChange(false)}
                                />
                            }
                        >
                            Детали тарифа
                        </DialogTitle>
                        <DialogContent className="p-4">
                            {isLoading && (
                                <div className="flex justify-center p-4">
                                    <Body1>Загрузка деталей...</Body1>
                                </div>
                            )}
                            {!isLoading && tariff && (
                                <div className="flex flex-col gap-4">

                                    <div className="flex flex-col gap-4">
                                        <div className="flex items-center gap-2">
                                            {tariff.themeEmoji && (
                                                <span className="text-3xl">{tariff.themeEmoji}</span>
                                            )}
                                            <Title2>{tariff.name}</Title2>
                                        </div>

                                        <Divider />

                                        <div className="flex flex-col gap-2">
                                            <Subtitle2>Описание</Subtitle2>
                                            <Body1>{tariff.description || "Нет описания"}</Body1>
                                        </div>

                                        <div className="flex flex-col gap-2">
                                            <Subtitle2>Тип тарификации</Subtitle2>
                                            <Body1>
                                                {tariff.billingType === BillingTypeEnum.PerMinute
                                                    ? "Поминутная"
                                                    : "Почасовая"}
                                            </Body1>
                                        </div>

                                        <div className="flex flex-col gap-2">
                                            <Subtitle2>Стоимость</Subtitle2>
                                            <Body1>
                                                {tariff.pricePerMinute} {CURRENCY_SYMBOL} / мин
                                                ({tariff.pricePerMinute * 60} {CURRENCY_SYMBOL} / час)
                                            </Body1>
                                        </div>

                                        {tariff.calculationExamples && tariff.calculationExamples.length > 0 && (
                                            <div className="flex flex-col gap-2">
                                                <Subtitle2>Примеры расчета</Subtitle2>
                                                <div className="grid grid-cols-1 gap-2">
                                                    {tariff.calculationExamples.map((example, index) => (
                                                        <div key={index} className="p-3 bg-(--colorNeutralBackground2) rounded-lg flex justify-between items-center">
                                                            <div className="flex flex-col">
                                                                <Body1 className="font-semibold">{example.actualMinutes} мин</Body1>
                                                                {example.billableMinutes !== example.actualMinutes && (
                                                                    <Body2 className="text-(--colorNeutralForeground3)">
                                                                        Оплачено: {example.billableMinutes} мин
                                                                    </Body2>
                                                                )}
                                                            </div>
                                                            <div className="flex flex-col items-end">
                                                                <Body1 className="font-semibold">{example.finalCost} {CURRENCY_SYMBOL}</Body1>
                                                                {example.optimizationGain > 0 && (
                                                                    <Caption1 className="text-(--colorPaletteGreenForeground1) font-medium">
                                                                        Выгода: {example.optimizationGain} {CURRENCY_SYMBOL}
                                                                    </Caption1>
                                                                )}
                                                            </div>
                                                        </div>
                                                    ))}
                                                </div>
                                            </div>
                                        )}

                                        {tariff.summary && (
                                            <div className="flex flex-col gap-2">
                                                <Subtitle2>Краткое описание</Subtitle2>
                                                <Body1>{tariff.summary}</Body1>
                                            </div>
                                        )}

                                        {tariff.features && tariff.features.length > 0 && (
                                            <div className="flex flex-col gap-2">
                                                <Subtitle2>Особенности</Subtitle2>
                                                <ul className="list-disc list-inside">
                                                    {tariff.features.map((feature, index) => (
                                                        <li key={index}><Body1>{feature}</Body1></li>
                                                    ))}
                                                </ul>
                                            </div>
                                        )}

                                        {tariff.audienceTags && tariff.audienceTags.length > 0 && (
                                            <div className="flex flex-col gap-2">
                                                <Subtitle2>Теги аудитории</Subtitle2>
                                                <div className="flex gap-2 flex-wrap">
                                                    {tariff.audienceTags.map((tag, index) => (
                                                        <span key={index} className="px-2 py-1 bg-(--colorNeutralBackground2) rounded-md text-sm">
                                                            {tag}
                                                        </span>
                                                    ))}
                                                </div>
                                            </div>
                                        )}

                                        {tariff.cancellationPolicy && (
                                            <div className="flex flex-col gap-2">
                                                <Subtitle2>Правила отмены</Subtitle2>
                                                <Body1>{tariff.cancellationPolicy}</Body1>
                                            </div>
                                        )}

                                        <div className="flex flex-col gap-2">
                                            <Subtitle2>Ограничения</Subtitle2>
                                            <Body1>Мин. время сессии: {tariff.minSessionMinutes ? `${tariff.minSessionMinutes} мин` : "Без ограничений"}</Body1>
                                            <Body1>Макс. гостей: {tariff.maxGuests || "Без ограничений"}</Body1>
                                        </div>
                                    </div>

                                </div>
                            )}
                        </DialogContent>
                    </DialogBody>
                </div>
            </DialogSurface>

        </Dialog >
    );
};
