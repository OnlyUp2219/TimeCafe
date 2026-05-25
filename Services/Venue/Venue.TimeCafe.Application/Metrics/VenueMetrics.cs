namespace Venue.TimeCafe.Application.Metrics;

public static class VenueMetrics
{
    public static readonly global::Prometheus.Gauge ActiveVisits = global::Prometheus.Metrics.CreateGauge("timecafe_active_visits_total", "Текущее количество активных визитов");
    public static readonly global::Prometheus.Gauge PendingVisits = global::Prometheus.Metrics.CreateGauge("timecafe_visits_pending_total", "Визитов в ожидании подтверждения");
    public static readonly global::Prometheus.Counter VisitsStarted = global::Prometheus.Metrics.CreateCounter("timecafe_visits_started_total", "Визитов начато");
    public static readonly global::Prometheus.Counter VisitsCompleted = global::Prometheus.Metrics.CreateCounter("timecafe_visits_completed_total", "Визитов завершено");
}
