import type {FC} from "react";
import {Badge, Body2, Button, Card, Title2, tokens} from "@fluentui/react-components";
import {ArrowExitFilled, ArrowExitRegular} from "@fluentui/react-icons";

export interface LogoutCardProps {
    className?: string;
    onLogout?: () => void;
}

export const LogoutCard: FC<LogoutCardProps> = ({className, onLogout}) => {
    return (
        <Card
            className={`${className || ""}`}
            style={{
                borderColor: tokens.colorPaletteRedBorder2,
            }}
        >
            <div className="flex h-full flex-col gap-2 justify-between">
                <div>
                    <Title2 block className="!flex items-center gap-2">
                        <Badge appearance="tint" shape="rounded" size="extra-large" className="dark-red-badge">
                            <ArrowExitRegular className="size-5" />
                        </Badge>
                        Выйти из аккаунта
                    </Title2>

                    <Body2 block>
                        Завершит текущую сессию и вернёт на экран входа.
                    </Body2>
                </div>

                <div className="flex flex-col sm:items-center sm:flex-row sm:justify-end">
                    <Button
                        appearance="primary"
                        onClick={onLogout}
                        className="dark-red-button w-full sm:w-auto "
                        icon={<ArrowExitFilled className="size-5" />}
                    >
                        Выйти
                    </Button>
                </div>
            </div>
        </Card>
    );
};
