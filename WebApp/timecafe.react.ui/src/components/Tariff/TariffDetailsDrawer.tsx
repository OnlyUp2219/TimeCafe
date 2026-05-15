import type { FC } from "react";
import {
    Body1,
    Body2,
    Button,
    Divider,
    Title2,
    OverlayDrawer,
    DrawerHeader,
    DrawerHeaderTitle,
    DrawerBody,
    Subtitle2,
    Card,
} from "@fluentui/react-components";
import { Dismiss20Regular } from "@fluentui/react-icons";
import { BillingType as BillingTypeEnum, type Tariff } from "@app-types/tariff";
import { useGetTariffByIdQuery } from "@store/api/venueApi";

interface TariffDetailsDrawerProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    tariffId: string | null;
}

export const TariffDetailsDrawer: FC<TariffDetailsDrawerProps> = ({
    open,
    onOpenChange,
    tariffId,
}) => {
    const { data: tariff, isLoading } = useGetTariffByIdQuery(tariffId ?? "", { skip: !tariffId });

    return (
        <OverlayDrawer
            size="large"
            position="bottom"
            className="!flex !flex-col !items-center"
            open={open}
            onOpenChange={(_, { open }) => onOpenChange(open)}
        >
            <div id="profileGateBackground" className="h-full w-full flex flex-col overflow-hidden" style={{ background: 'linear-gradient(135deg, #f7fafc 0%, #eef4ff 45%, #eafbf7 100%)' }}>
                <div className="profile-gate-bg" aria-hidden="true">
                    <span className="profile-gate-shape profile-gate-shape--circle" />
                    <span className="profile-gate-shape profile-gate-shape--square" />
                    <span className="profile-gate-shape profile-gate-shape--triangle" />
                </div>
                <DrawerHeader className="z-10">
                    <DrawerHeaderTitle
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
                    </DrawerHeaderTitle>
                </DrawerHeader>
                <DrawerBody className="z-10">
                    {isLoading && (
                        <div className="flex justify-center p-4">
                            <Body1>Загрузка деталей...</Body1>
                        </div>
                    )}
                    {!isLoading && tariff && (
                        <div className="p-4">
                            <Card size="large" className="w-full">
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
                                            {tariff.pricePerMinute} ₽ / мин
                                            ({tariff.pricePerMinute * 60} ₽ / час)
                                        </Body1>
                                    </div>

                                    {tariff.calculationExamples && tariff.calculationExamples.length > 0 && (
                                        <div className="flex flex-col gap-2">
                                            <Subtitle2>Примеры расчета</Subtitle2>
                                            <div className="grid grid-cols-1 gap-2">
                                                {tariff.calculationExamples.map((example, index) => (
                                                    <div key={index} className="p-3 bg-[var(--colorNeutralBackground2)] rounded-lg flex justify-between items-center">
                                                        <div className="flex flex-col">
                                                            <Body1 className="font-semibold">{example.actualMinutes} мин</Body1>
                                                            {example.billableMinutes !== example.actualMinutes && (
                                                                <Body2 className="text-[var(--colorNeutralForeground3)]">
                                                                    Оплачено: {example.billableMinutes} мин
                                                                </Body2>
                                                            )}
                                                        </div>
                                                        <div className="flex flex-col items-end">
                                                            <Body1 className="font-semibold">{example.finalCost} ₽</Body1>
                                                            {example.optimizationGain > 0 && (
                                                                <Body2 className="text-[var(--colorPaletteGreenForeground1)]">
                                                                    Выгода: {example.optimizationGain} ₽
                                                                </Body2>
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
                                                    <span key={index} className="px-2 py-1 bg-[var(--colorNeutralBackground2)] rounded-md text-sm">
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
                            </Card>
                        </div>
                    )}
                </DrawerBody>
            </div>
        </OverlayDrawer>
    );
};
