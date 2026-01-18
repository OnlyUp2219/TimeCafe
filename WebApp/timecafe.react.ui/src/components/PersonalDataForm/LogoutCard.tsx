import type {FC} from "react";
import {Button, Card, Body2, Title2} from "@fluentui/react-components";
import {ArrowExitFilled} from "@fluentui/react-icons";

export interface LogoutCardProps {
    className?: string;
    onLogout?: () => void;
}

export const LogoutCard: FC<LogoutCardProps> = ({className, onLogout}) => {
    return (
        <Card className={`h-full ${className || ''}`}>
            <div className="flex h-full flex-col gap-2 justify-between">
                <div>
                    <Title2 block>Выйти из аккаунта</Title2>
                    <Body2 block>
                        Завершит текущую сессию и вернёт на экран входа.
                    </Body2>
                </div>

                <div className="flex flex-col sm:items-center sm:flex-row">
                    <Button
                        appearance="primary"
                        onClick={onLogout}
                        className="dark-red"
                        icon={<ArrowExitFilled/>}
                    >
                        Выйти
                    </Button>
                </div>
            </div>
        </Card>
    );
};
