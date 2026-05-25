namespace Auth.TimeCafe.Application.Metrics;

public static class AuthMetrics
{
    public static readonly global::Prometheus.Gauge RegisteredUsers = global::Prometheus.Metrics.CreateGauge("timecafe_registered_users_total", "Зарегистрированных пользователей");
}
