export const authStatusColor = (status: string): "success" | "danger" | "warning" => {
    switch (status.toLowerCase()) {
        case "active": return "success";
        case "inactive": return "danger";
        default: return "warning";
    }
};
