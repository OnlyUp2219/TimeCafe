import type {TariffCarouselSectionProps} from "@components/Tariff/TariffCarouselSectionProps.ts";
import {
    Body2,
    Card,
    Carousel,
    CarouselCard,
    CarouselNav,
    CarouselNavButton,
    CarouselNavContainer,
    CarouselSlider,
    CarouselViewport,
    Divider,
    Title3
} from "@fluentui/react-components";
import {TariffCard} from "@components/TariffCard/TariffCard.tsx";

export const TariffCarouselSection = ({
                                          visibleTariffs,
                                          totalCount,
                                          activeIndex,
                                          onActiveIndexChange,
                                          selectedTariffId,
                                          onSelectTariff,
                                      }: TariffCarouselSectionProps) => {
    return (
        <Card className="flex flex-col gap-3">
            <div className="flex items-center justify-between gap-3 flex-wrap">
                <Title3>Тарифы</Title3>
                <Body2 block>Потяните мышкой/тачем или используйте стрелки.</Body2>
            </div>

            <Divider/>

            <Carousel
                activeIndex={activeIndex}
                groupSize={1}
                onActiveIndexChange={(_, data) => onActiveIndexChange(data.index)}
                circular
                draggable
                align="center"
                whitespace
            >
                <CarouselViewport>
                    <CarouselSlider cardFocus className="tc-carousel-slider">
                        {visibleTariffs.map((tariff, index) => (
                            <CarouselCard
                                key={tariff.tariffId}
                                autoSize
                                aria-label={`${index + 1} из ${totalCount}`}
                            >
                                <div
                                    className={
                                        index === activeIndex
                                            ? "scale-100 opacity-100"
                                            : "scale-[0.92] opacity-[0.55]"
                                    }
                                >
                                    <TariffCard
                                        tariff={tariff}
                                        selected={tariff.tariffId === selectedTariffId}
                                        onSelect={onSelectTariff}
                                    />
                                </div>
                            </CarouselCard>
                        ))}
                    </CarouselSlider>
                </CarouselViewport>

                <CarouselNavContainer
                    next={{"aria-label": "следующий тариф"}}
                    prev={{"aria-label": "предыдущий тариф"}}
                >
                    <CarouselNav>
                        {(index) => <CarouselNavButton aria-label={`Тариф ${index + 1}`}/>}
                    </CarouselNav>
                </CarouselNavContainer>
            </Carousel>
        </Card>
    );
};