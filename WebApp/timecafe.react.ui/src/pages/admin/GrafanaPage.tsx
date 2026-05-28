import React from "react";
import { Title2, Subtitle1, Card } from "@fluentui/react-components";

export const GrafanaPage: React.FC = () => {
    return (
        <div className="flex flex-col gap-6 p-4 w-full h-full min-h-[calc(100vh-120px)]">
            <div className="flex flex-col gap-1">
                <Title2>Мониторинг Grafana</Title2>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 w-full">
                <Card className="p-3 flex flex-col gap-2 h-[180px]">
                    <Subtitle1>Активных визитов</Subtitle1>
                    <iframe
                        src="http://localhost:3000/d-solo/timecafe/overview?panelId=1&theme=light"
                        width="100%"
                        height="100%"
                        style={{ border: "none", borderRadius: "var(--borderRadiusMedium)", flexGrow: 1 }}
                        title="Active Visits"
                    />
                </Card>
                <Card className="p-3 flex flex-col gap-2 h-[180px]">
                    <Subtitle1>Ожидают подтверждения</Subtitle1>
                    <iframe
                        src="http://localhost:3000/d-solo/timecafe/overview?panelId=2&theme=light"
                        width="100%"
                        height="100%"
                        style={{ border: "none", borderRadius: "var(--borderRadiusMedium)", flexGrow: 1 }}
                        title="Pending Visits"
                    />
                </Card>
                <Card className="p-3 flex flex-col gap-2 h-[180px]">
                    <Subtitle1>Выручка (₽)</Subtitle1>
                    <iframe
                        src="http://localhost:3000/d-solo/timecafe/overview?panelId=3&theme=light"
                        width="100%"
                        height="100%"
                        style={{ border: "none", borderRadius: "var(--borderRadiusMedium)", flexGrow: 1 }}
                        title="Revenue"
                    />
                </Card>
                <Card className="p-3 flex flex-col gap-2 h-[180px]">
                    <Subtitle1>Пользователей</Subtitle1>
                    <iframe
                        src="http://localhost:3000/d-solo/timecafe/overview?panelId=4&theme=light"
                        width="100%"
                        height="100%"
                        style={{ border: "none", borderRadius: "var(--borderRadiusMedium)", flexGrow: 1 }}
                        title="Users Count"
                    />
                </Card>
            </div>

            <div className="grid grid-cols-1 xl:grid-cols-3 gap-6 w-full flex-1">
                <Card className="p-4 flex flex-col gap-3 min-h-[450px]">
                    <Subtitle1>HTTP Latency P95 (Grafana)</Subtitle1>
                    <iframe
                        src="http://localhost:3000/d-solo/timecafe/overview?panelId=5&theme=light"
                        width="100%"
                        height="100%"
                        style={{ border: "none", borderRadius: "var(--borderRadiusXLarge)", flexGrow: 1, minHeight: "350px" }}
                        title="Grafana Latency"
                    />
                </Card>

                <Card className="p-4 flex flex-col gap-3 min-h-[450px]">
                    <Subtitle1>HTTP Requests per Second (Grafana)</Subtitle1>
                    <iframe
                        src="http://localhost:3000/d-solo/timecafe/overview?panelId=6&theme=light"
                        width="100%"
                        height="100%"
                        style={{ border: "none", borderRadius: "var(--borderRadiusXLarge)", flexGrow: 1, minHeight: "350px" }}
                        title="Grafana RPS"
                    />
                </Card>

                <Card className="p-4 flex flex-col gap-3 min-h-[450px]">
                    <Subtitle1>Визиты за 24ч (Grafana)</Subtitle1>
                    <iframe
                        src="http://localhost:3000/d-solo/timecafe/overview?panelId=7&theme=light"
                        width="100%"
                        height="100%"
                        style={{ border: "none", borderRadius: "var(--borderRadiusXLarge)", flexGrow: 1, minHeight: "350px" }}
                        title="Grafana Visits"
                    />
                </Card>
            </div>
        </div>
    );
};
