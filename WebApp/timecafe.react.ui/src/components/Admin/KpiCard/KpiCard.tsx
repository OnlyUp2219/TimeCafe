import {Card, Title2, Body2} from "@fluentui/react-components";
import {useComponentSize} from "@hooks/useComponentSize";

interface KpiCardProps {
    title: string;
    value: string | number;
    icon: React.ReactElement;
    onClick?: () => void;
}

export const KpiCard = ({title, value, icon, onClick}: KpiCardProps) => {
    const {sizes} = useComponentSize();
    return (
        <Card
            className={`flex-1 min-w-[200px] ${onClick ? "cursor-pointer" : ""}`}
            size={sizes.card}
            onClick={onClick}
        >
            <div className="flex items-center justify-between">
                <div className="min-w-0 flex-1">
                    <Body2 className="line-clamp-3">{title}</Body2>
                    <Title2>{value}</Title2>
                </div>
                <div className="text-2xl opacity-50 shrink-0 ml-2">{icon}</div>
            </div>
        </Card>
    );
};
