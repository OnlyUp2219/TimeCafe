using FluentAssertions;
using Venue.TimeCafe.Domain.Models;
using Xunit;

namespace Venue.TimeCafe.Test.Unit.Domain;

public class VisitTests
{
    [Fact]
    public void CalculateCost_Should_ApplyOnlyPersonalDiscount()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(100); // 100 minutes
        var pricePerMinute = 1m; // 100 total base cost
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime,
            personalDiscount: 10m // 10%
        );

        // Assert
        cost.ActualMinutes.Should().Be(100);
        cost.BillableMinutes.Should().Be(100);
        cost.BaseCost.Should().Be(100m);
        cost.FinalCost.Should().Be(90m);
        cost.OptimizationGain.Should().Be(-10m);
    }

    [Fact]
    public void CalculateCost_Should_ApplyPersonalAndTariffDiscount_WhenTariffIsBetterThanGlobal()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(100);
        var pricePerMinute = 1m;
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime,
            personalDiscount: 10m, // 10%
            tariffDiscount: 20m,   // 20% (Better)
            globalDiscount: 15m    // 15%
        );

        // Assert
        cost.ActualMinutes.Should().Be(100);
        cost.BillableMinutes.Should().Be(100);
        cost.BaseCost.Should().Be(100m);
        cost.FinalCost.Should().Be(70m);
        cost.OptimizationGain.Should().Be(-30m);
    }

    [Fact]
    public void CalculateCost_Should_ApplyPersonalAndGlobalDiscount_WhenGlobalIsBetterThanTariff()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(100);
        var pricePerMinute = 1m;
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime,
            personalDiscount: 10m, // 10%
            tariffDiscount: 15m,   // 15%
            globalDiscount: 25m    // 25% (Better)
        );

        // Assert
        cost.ActualMinutes.Should().Be(100);
        cost.BillableMinutes.Should().Be(100);
        cost.BaseCost.Should().Be(100m);
        cost.FinalCost.Should().Be(65m);
        cost.OptimizationGain.Should().Be(-35m);
    }

    [Fact]
    public void CalculateCost_Should_CapDiscount_AtMaxDiscountPercent()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(100);
        var pricePerMinute = 1m;
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime,
            personalDiscount: 30m,
            tariffDiscount: 30m,
            maxDiscountPercent: 50m // Cap at 50%
        );

        // Assert
        cost.ActualMinutes.Should().Be(100);
        cost.BillableMinutes.Should().Be(100);
        cost.BaseCost.Should().Be(100m);
        cost.FinalCost.Should().Be(50m);
        cost.OptimizationGain.Should().Be(-50m);
    }

    [Fact]
    public void CalculateCost_Should_CalculateHourlyCorrectly()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(65); // 1 hour and 5 minutes -> should be 2 hours
        var pricePerMinute = 1m; // 60 per hour
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.Hourly,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime
        );

        // Assert
        cost.ActualMinutes.Should().Be(65);
        cost.BillableMinutes.Should().Be(120);
        cost.BaseCost.Should().Be(120m);
        cost.FinalCost.Should().Be(120m);
        cost.OptimizationGain.Should().Be(0m);
    }

    [Fact]
    public void CalculateCost_Should_CalculatePerMinuteCorrectly()
    {
        // Arrange
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(65.5); // 65.5 minutes -> should be 66 minutes
        var pricePerMinute = 1m;
        
        // Act
        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime
        );

        // Assert
        cost.ActualMinutes.Should().Be(66);
        cost.BillableMinutes.Should().Be(66);
        cost.BaseCost.Should().Be(66m);
        cost.FinalCost.Should().Be(66m);
        cost.OptimizationGain.Should().Be(0m);
    }

    [Fact]
    public void Approve_Should_Fail_WhenAlreadyApproved()
    {
        var visit = new Visit { Status = VisitStatus.Pending };
        visit.Approve(Guid.NewGuid()).IsSuccess.Should().BeTrue();

        var secondApprove = visit.Approve(Guid.NewGuid());

        secondApprove.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Approve_Should_Fail_WhenAlreadyRejected()
    {
        var visit = new Visit { Status = VisitStatus.Pending };
        visit.Reject("Нет мест").IsSuccess.Should().BeTrue();

        var approveResult = visit.Approve(Guid.NewGuid());

        approveResult.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Approve_Should_Fail_WhenAlreadyCancelled()
    {
        var visit = new Visit { Status = VisitStatus.Pending };
        visit.Cancel().IsSuccess.Should().BeTrue();

        var approveResult = visit.Approve(Guid.NewGuid());

        approveResult.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Approve_Should_Fail_WhenAlreadyCompleted()
    {
        var visit = new Visit { Status = VisitStatus.Pending };
        visit.Approve(Guid.NewGuid()).IsSuccess.Should().BeTrue();
        visit.Status = VisitStatus.Completed;

        var approveResult = visit.Approve(Guid.NewGuid());

        approveResult.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Reject_Should_Fail_WhenAlreadyRejected()
    {
        var visit = new Visit { Status = VisitStatus.Pending };
        visit.Reject("Причина").IsSuccess.Should().BeTrue();

        var secondReject = visit.Reject("Другая причина");

        secondReject.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Reject_Should_Fail_WhenAlreadyActive()
    {
        var visit = new Visit { Status = VisitStatus.Pending };
        visit.Approve(Guid.NewGuid()).IsSuccess.Should().BeTrue();

        var rejectResult = visit.Reject("Причина");

        rejectResult.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Cancel_Should_Fail_WhenAlreadyCancelled()
    {
        var visit = new Visit { Status = VisitStatus.Pending };
        visit.Cancel().IsSuccess.Should().BeTrue();

        var secondCancel = visit.Cancel();

        secondCancel.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Cancel_Should_Fail_WhenAlreadyActive()
    {
        var visit = new Visit { Status = VisitStatus.Pending };
        visit.Approve(Guid.NewGuid()).IsSuccess.Should().BeTrue();

        var cancelResult = visit.Cancel();

        cancelResult.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Cancel_Should_Fail_WhenAlreadyCompleted()
    {
        var visit = new Visit { Status = VisitStatus.Active };
        visit.Status = VisitStatus.Completed;

        var cancelResult = visit.Cancel();

        cancelResult.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Approve_Should_SetStatusToActive()
    {
        var visit = new Visit { Status = VisitStatus.Pending };
        var approverId = Guid.NewGuid();

        var result = visit.Approve(approverId);

        result.IsSuccess.Should().BeTrue();
        visit.Status.Should().Be(VisitStatus.Active);
        visit.ApprovedByUserId.Should().Be(approverId);
        visit.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_Should_SetStatusToRejected()
    {
        var visit = new Visit { Status = VisitStatus.Pending };

        var result = visit.Reject("Нет свободных мест");

        result.IsSuccess.Should().BeTrue();
        visit.Status.Should().Be(VisitStatus.Rejected);
        visit.RejectionReason.Should().Be("Нет свободных мест");
    }

    [Fact]
    public void CalculateCost_Should_ApplyMinSessionMinutes()
    {
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(10);
        var pricePerMinute = 1m;

        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime,
            minSessionMinutes: 30
        );

        cost.ActualMinutes.Should().Be(10);
        cost.BillableMinutes.Should().Be(30);
        cost.BaseCost.Should().Be(10m);
        cost.FinalCost.Should().Be(30m);
    }

    [Fact]
    public void CalculateCost_Should_ApplyRoundingRule()
    {
        var entryTime = DateTimeOffset.UtcNow;
        var exitTime = entryTime.AddMinutes(7);
        var pricePerMinute = 1m;

        var cost = Visit.CalculateCost(
            tariffBillingType: BillingType.PerMinute,
            tariffPricePerMinute: pricePerMinute,
            exitTime: exitTime,
            entryTime: entryTime,
            roundingRule: "5min"
        );

        cost.ActualMinutes.Should().Be(7);
        cost.BillableMinutes.Should().Be(10);
        cost.BaseCost.Should().Be(7m);
        cost.FinalCost.Should().Be(10m);
    }
}
