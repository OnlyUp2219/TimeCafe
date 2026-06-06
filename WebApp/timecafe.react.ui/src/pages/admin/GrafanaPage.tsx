import React from "react";
import { Title2, Subtitle1, Card } from "@fluentui/react-components";

export const GrafanaPage: React.FC = () => {
    return (
        <div className="flex flex-col gap-6 p-4 w-full h-full min-h-[calc(100vh-120px)]">
            <div className="flex flex-col gap-1">
                <Title2>Мониторинг Grafana</Title2>
            </div>

            <div className="flex-1 w-full rounded-2xl overflow-hidden border border-(--colorNeutralStroke1) shadow-sm bg-(--colorNeutralBackground2)">
                <iframe
                    src="http://localhost:3000/d/timecafe/overview?kiosk&theme=light"
                    width="100%"
                    height="100%"
                    style={{ border: "none", minHeight: "75vh" }}
                    title="Grafana Dashboard"
                    className="w-full h-full"
                />
            </div>
        </div>
    );
};
