namespace Venue.TimeCafe.Domain.Models;

public class Visit
{
    public Visit()
    {
        VisitId = Guid.NewGuid();
    }

    public Visit(Guid visitId)
    {
        VisitId = visitId;
    }

    public Guid VisitId { get; set; }
    public Guid? UserId { get; set; }
    public Guid TariffId { get; set; }
    public Guid? ResourceId { get; set; }
    public DateTimeOffset EntryTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExitTime { get; set; }
    public decimal? CalculatedCost { get; set; }
    public VisitStatus Status { get; set; } = VisitStatus.Pending;
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public int? PlannedMinutes { get; set; }
    public int GuestsCount { get; set; } = 1;
    public bool IsFinishRequested { get; set; } = false;
    public DateTimeOffset? FinishRequestedAt { get; set; }

    public static Visit Create(Guid? visitId, Guid? userId, Guid tariffId, DateTimeOffset entryTime, VisitStatus status, Guid? resourceId = null, DateTimeOffset? exitTime = null, decimal? calculatedCost = null, int? plannedMinutes = null, int guestsCount = 1)
    {
        return new Visit
        {
            VisitId = visitId ?? Guid.NewGuid(),
            UserId = userId,
            TariffId = tariffId,
            ResourceId = resourceId,
            EntryTime = entryTime,
            ExitTime = exitTime,
            CalculatedCost = calculatedCost,
            Status = status,
            PlannedMinutes = plannedMinutes,
            GuestsCount = guestsCount
        };
    }

    public static Visit Update(Visit existingVisit, Guid? userId = null, Guid? tariffId = null, Guid? resourceId = null, DateTimeOffset? entryTime = null, DateTimeOffset? exitTime = null, decimal? calculatedCost = null, VisitStatus? status = null, Guid? approvedByUserId = null, DateTimeOffset? approvedAt = null, string? rejectionReason = null, int? plannedMinutes = null, int? guestsCount = null, bool? isFinishRequested = null)
    {
        return new Visit(existingVisit.VisitId)
        {
            UserId = userId ?? existingVisit.UserId,
            TariffId = tariffId ?? existingVisit.TariffId,
            ResourceId = resourceId ?? existingVisit.ResourceId,
            EntryTime = entryTime ?? existingVisit.EntryTime,
            ExitTime = exitTime ?? existingVisit.ExitTime,
            CalculatedCost = calculatedCost ?? existingVisit.CalculatedCost,
            Status = status ?? existingVisit.Status,
            ApprovedByUserId = approvedByUserId ?? existingVisit.ApprovedByUserId,
            ApprovedAt = approvedAt ?? existingVisit.ApprovedAt,
            RejectionReason = rejectionReason ?? existingVisit.RejectionReason,
            PlannedMinutes = plannedMinutes ?? existingVisit.PlannedMinutes,
            GuestsCount = guestsCount ?? existingVisit.GuestsCount,
            IsFinishRequested = isFinishRequested ?? existingVisit.IsFinishRequested
        };
    }

    public FluentResults.Result Approve(Guid approvedByUserId)
    {
        if (Status != VisitStatus.Pending)
            return FluentResults.Result.Fail(new Errors.VisitNotPendingError());

        Status = VisitStatus.Active;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTimeOffset.UtcNow;
        EntryTime = DateTimeOffset.UtcNow;
        return FluentResults.Result.Ok();
    }

    public FluentResults.Result Reject(string reason)
    {
        if (Status != VisitStatus.Pending)
            return FluentResults.Result.Fail(new Errors.VisitNotPendingError());

        Status = VisitStatus.Rejected;
        RejectionReason = reason;
        return FluentResults.Result.Ok();
    }

    public FluentResults.Result Cancel()
    {
        if (Status != VisitStatus.Pending)
            return FluentResults.Result.Fail(new Errors.VisitCannotBeCancelledError());

        Status = VisitStatus.Cancelled;
        return FluentResults.Result.Ok();
    }

    public FluentResults.Result RequestFinish()
    {
        if (Status != VisitStatus.Active)
            return FluentResults.Result.Fail(new Errors.VisitNotActiveError());

        IsFinishRequested = true;
        FinishRequestedAt = DateTimeOffset.UtcNow;
        return FluentResults.Result.Ok();
    }

    public FluentResults.Result FixateTime(decimal calculatedCost, DateTimeOffset exitTime)
    {
        if (Status != VisitStatus.Active)
            return FluentResults.Result.Fail(new Errors.VisitNotActiveError());

        Status = VisitStatus.WaitingForPayment;
        ExitTime = exitTime;
        CalculatedCost = calculatedCost;
        return FluentResults.Result.Ok();
    }

    public FluentResults.Result Complete()
    {
        if (Status != VisitStatus.WaitingForPayment)
            return FluentResults.Result.Fail(new Errors.VisitNotActiveError());

        Status = VisitStatus.Completed;
        return FluentResults.Result.Ok();
    }


    public static CostBreakdown CalculateCost(BillingType tariffBillingType, decimal tariffPricePerMinute, DateTimeOffset exitTime, DateTimeOffset entryTime, int? minSessionMinutes = null, string? roundingRule = null, decimal maxDiscountPercent = 50m, decimal globalDiscount = 0m, decimal tariffDiscount = 0m, decimal personalDiscount = 0m)
    {
        var actualDuration = (exitTime - entryTime).TotalMinutes;
        var actualMinutes = (int)Math.Ceiling(actualDuration);

        var duration = actualDuration;

        if (minSessionMinutes.HasValue && duration < minSessionMinutes.Value)
        {
            duration = minSessionMinutes.Value;
        }

        var roundInterval = roundingRule switch
        {
            Constants.TariffRoundingRules.FiveMinutes => 5,
            Constants.TariffRoundingRules.FifteenMinutes => 15,
            Constants.TariffRoundingRules.SixtyMinutes => 60,
            _ => 1
        };

        if (roundInterval > 1)
        {
            duration = Math.Ceiling(duration / roundInterval) * roundInterval;
        }

        var billableMinutes = tariffBillingType == BillingType.Hourly
                ? (int)Math.Ceiling(duration / 60) * 60
                : (int)Math.Ceiling(duration);

        var pureTimeCost = tariffBillingType == BillingType.Hourly
                ? (decimal)Math.Ceiling(actualDuration / 60) * tariffPricePerMinute * 60
                : (decimal)Math.Ceiling(actualDuration) * tariffPricePerMinute;

        var billableCost = tariffBillingType == BillingType.Hourly
                ? (decimal)Math.Ceiling(duration / 60) * tariffPricePerMinute * 60
                : (decimal)Math.Ceiling(duration) * tariffPricePerMinute;

        var bestPromotion = Math.Max(globalDiscount, tariffDiscount);
        var totalDiscount = Math.Min(bestPromotion + personalDiscount, maxDiscountPercent);

        var finalCost = billableCost * (1m - totalDiscount / 100m);

        return new CostBreakdown
        {
            ActualMinutes = actualMinutes,
            BillableMinutes = billableMinutes,
            BaseCost = pureTimeCost,
            FinalCost = finalCost,
            OptimizationGain = finalCost - pureTimeCost
        };
    }
}

