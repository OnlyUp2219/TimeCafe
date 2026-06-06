import React from "react";
import { Title2, Subtitle1, Card } from "@fluentui/react-components";

export const KibanaPage: React.FC = () => {
    return (
        <div className="flex flex-col gap-6 p-4 w-full h-full min-h-[calc(100vh-120px)]">
            <div className="flex flex-col gap-1">
                <Title2>Логи системы Kibana</Title2>
            </div>

            <iframe
                src="http://localhost:5601/app/discover#/?_g=(filters:!(),refreshInterval:(pause:!t,value:0),time:(from:now-24h,to:now))&_a=(columns:!(),filters:!(),index:'*',interval:auto,query:(language:kuery,query:'level:%20error'),sort:!(!('@timestamp',desc)))"
                width="100%"
                height="100%"
                style={{ border: "none", borderRadius: "var(--borderRadiusXLarge)", flexGrow: 1, minHeight: "550px" }}
                title="Kibana Errors"
            />
        </div>
    );
};
