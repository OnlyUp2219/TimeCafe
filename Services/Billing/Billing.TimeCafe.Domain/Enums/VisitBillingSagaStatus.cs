namespace Billing.TimeCafe.Domain.Enums;

public enum VisitBillingSagaStatus
{
    Pending = 1,
    Completed = 2,
    Compensated = 3,
    Failed = 4
}