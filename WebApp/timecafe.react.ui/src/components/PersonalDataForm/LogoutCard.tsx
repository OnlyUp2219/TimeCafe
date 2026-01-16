import type {FC} from "react";
import {Button, Card, Body2, Title2, tokens} from "@fluentui/react-components";

export interface LogoutCardProps {
    className?: string;
    onLogout?: () => void;
}

export const LogoutCard: FC<LogoutCardProps> = ({className, onLogout}) => {
    return (
        <Card className={className}>
            <div className="flex flex-col gap-2">
                <Title2>Выйти из аккаунта</Title2>
                <Body2 style={{color: tokens.colorNeutralForeground2}}>
                    Завершит текущую сессию и вернёт на экран входа.
                </Body2>

                <div>
                    <Button
                        appearance="primary"
                        onClick={onLogout}
                        className="dark-red"
                    >
                        Выйти
                    </Button>
                </div>
            </div>
        </Card>
    );
};
