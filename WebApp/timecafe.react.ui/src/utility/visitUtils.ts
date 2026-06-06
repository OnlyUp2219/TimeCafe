import { VisitStatus } from "@app-types/visit";

export const visitStatusLabel = (s: number) => s === VisitStatus.Active ? "Активен" : "Завершён";
export const visitStatusColor = (s: number): "success" | "informative" => s === VisitStatus.Active ? "success" : "informative";
