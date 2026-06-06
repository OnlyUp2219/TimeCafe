import {Body2, Caption1, Card, Subtitle2Stronger} from "@fluentui/react-components";
import {ChatRegular, CallRegular} from "@fluentui/react-icons";
import {TooltipButton} from "@components/TooltipButton/TooltipButton";
import {useComponentSize} from "@hooks/useComponentSize";

type SupportCardProps = {
    telegramUrl: string;
    onCallAdmin: () => void;
};

export const SupportCard = ({telegramUrl, onCallAdmin}: SupportCardProps) => {
    const { sizes } = useComponentSize();

    return (
        <Card className="flex h-full flex-col gap-4" size={sizes.card}>
            <div className="flex items-center gap-2">
                <ChatRegular />
                <Caption1 block className="uppercase">
                    Поддержка
                </Caption1>
            </div>

            <div className="flex flex-col gap-2">
                <Subtitle2Stronger block>Нужна помощь?</Subtitle2Stronger>
                <Body2 block>Администратор на связи и готов ответить на ваши вопросы по работе антикафе.</Body2>
            </div>

            <div className="flex flex-wrap gap-2">
                <TooltipButton
                    appearance="primary"
                    icon={<ChatRegular />}
                    tooltip="Открыть чат в Telegram"
                    label="Чат в Telegram"
                    onClick={() => globalThis.open(telegramUrl, "_blank")}
                    size={sizes.button}
                />
                <TooltipButton
                    appearance="secondary"
                    icon={<CallRegular />}
                    tooltip="Позвать администратора"
                    label="Позвать админа"
                    onClick={onCallAdmin}
                    size={sizes.button}
                />
            </div>
        </Card>
    );
};
