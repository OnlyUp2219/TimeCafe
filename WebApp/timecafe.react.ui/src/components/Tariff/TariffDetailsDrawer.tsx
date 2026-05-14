import type { FC } from "react";
import {
    Body1,
    Button,
    Divider,
    Title2,
    OverlayDrawer,
    DrawerHeader,
    DrawerHeaderTitle,
    DrawerBody,
    Subtitle2,
} from "@fluentui/react-components";
import { Dismiss20Regular } from "@fluentui/react-icons";
import { BillingType as BillingTypeEnum, type Tariff } from "@app-types/tariff";

interface TariffDetailsDrawerProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    tariff: Tariff | null;
}

export const TariffDetailsDrawer: FC<TariffDetailsDrawerProps> = ({
    open,
    onOpenChange,
    tariff,
}) => {
    return (
        <OverlayDrawer
            size="full"
            open={open}
            onOpenChange={(_, { open }) => onOpenChange(open)}
        >
            <DrawerHeader>
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
            <DrawerBody>
                {tariff && (
                    <div className="flex flex-col gap-4 p-4">
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
                            <Body1>Мин. время сессии: {tariff.minSessionMinutes} мин</Body1>
                            <Body1>Макс. гостей: {tariff.maxGuests || "Без ограничений"}</Body1>
                        </div>
                    </div>
                )}
            </DrawerBody>
        </OverlayDrawer>
    );
};
