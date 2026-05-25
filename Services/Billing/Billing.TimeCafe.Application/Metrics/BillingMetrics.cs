namespace Billing.TimeCafe.Application.Metrics;

public static class BillingMetrics
{
    public static readonly global::Prometheus.Counter Revenue = global::Prometheus.Metrics.CreateCounter("timecafe_revenue_total", "Суммарная выручка", "currency");
    public static readonly global::Prometheus.Counter SuccessfulPayments = global::Prometheus.Metrics.CreateCounter("timecafe_successful_payments_total", "Количество успешных платежей");
    public static readonly global::Prometheus.Counter Refunds = global::Prometheus.Metrics.CreateCounter("timecafe_refunds_total", "Количество возвратов средств");
    public static readonly global::Prometheus.Counter Adjustments = global::Prometheus.Metrics.CreateCounter("timecafe_adjustments_total", "Количество ручных корректировок баланса");
}
