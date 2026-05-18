namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Queries;

public class GetPaymentHistoryQueryWithRealStripeTests : BasePaymentTest
{
    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnPaymentsFromRealStripe_WhenUserHasPayments()
    {
        // Arrange
        var userId = DefaultsGuid.UserId;

        await CreatePaymentAsync(
            DefaultsGuid.PaymentId,
            userId,
            DefaultsGuid.DefaultAmount,
            PaymentStatus.Completed,
            externalPaymentId: StripeTestData.PaymentIntents.RealTest1);

        await CreatePaymentAsync(
            DefaultsGuid.PaymentId2,
            userId,
            DefaultsGuid.SmallAmount,
            PaymentStatus.Completed,
            externalPaymentId: StripeTestData.PaymentIntents.RealTest2);

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Value.Payments.Should().Contain(p => p.ExternalPaymentId == StripeTestData.PaymentIntents.RealTest1);
        result.Value.Payments.Should().Contain(p => p.ExternalPaymentId == StripeTestData.PaymentIntents.RealTest2);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnEmptyList_WhenUserHasNoPayments()
    {
        // Arrange
        var userId = DefaultsGuid.UserId2;

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_FilterByUserIdCorrectly_WhenMultipleUsersHavePayments()
    {
        // Arrange
        var userId1 = DefaultsGuid.UserId;
        var userId2 = DefaultsGuid.UserId2;

        await CreatePaymentAsync(DefaultsGuid.PaymentId, userId1, DefaultsGuid.DefaultAmount, externalPaymentId: StripeTestData.PaymentIntents.RealUser1);
        await CreatePaymentAsync(DefaultsGuid.PaymentId2, userId2, DefaultsGuid.SmallAmount, externalPaymentId: StripeTestData.PaymentIntents.RealUser2);

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId1));
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        var userPayments = result.Value.Payments!.Where(p => p.ExternalPaymentId == StripeTestData.PaymentIntents.RealUser1);
        userPayments.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Value.Payments.Should().NotContain(p => p.ExternalPaymentId == StripeTestData.PaymentIntents.RealUser2);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_HandlePaginationCorrectly_WhenRequestingDifferentPages()
    {
        // Arrange
        var userId = DefaultsGuid.UserId;
        for (int i = 0; i < 15; i++)
        {
            await CreatePaymentAsync(
                new Guid($"00000000-0000-0000-0000-{i:000000000000}"),
                userId,
                DefaultsGuid.DefaultAmount + i,
                externalPaymentId: $"pi_test_{i}");
        }

        Result<GetPaymentHistoryResponse> page1Result;
        Result<GetPaymentHistoryResponse> page2Result;

        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            page1Result = await sender.Send(new GetPaymentHistoryQuery(userId, 1, 5));
            page2Result = await sender.Send(new GetPaymentHistoryQuery(userId, 2, 5));
        }

        // Assert
        page1Result.Value.Payments.Should().HaveCount(5);
        page2Result.Value.Payments.Should().HaveCount(5);
        page1Result.Value.Payments.Should().NotIntersectWith(page2Result.Value.Payments);
        page1Result.Value.TotalCount.Should().BeGreaterThanOrEqualTo(15);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_IncludeStripeMetadata_InPaymentDetails()
    {
        // Arrange
        var userId = DefaultsGuid.UserId;
        var externalPaymentId = StripeTestData.PaymentIntents.MetadataTest;

        await CreatePaymentAsync(
            DefaultsGuid.PaymentId,
            userId,
            DefaultsGuid.DefaultAmount,
            PaymentStatus.Completed,
            externalPaymentId: externalPaymentId);

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        // Assert
        var payment = result.Value.Payments?.FirstOrDefault(p => p.ExternalPaymentId == externalPaymentId);
        payment.Should().NotBeNull();
        payment!.ExternalPaymentId.Should().StartWith("pi_");
        payment.Amount.Should().Be(DefaultsGuid.DefaultAmount);
        payment.Status.Should().Be(nameof(PaymentStatus.Completed));
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_IncludeTimestamps_ForEachPayment()
    {
        // Arrange
        var userId = DefaultsGuid.UserId;
        var beforeCreation = DateTime.UtcNow;

        await CreatePaymentAsync(
            DefaultsGuid.PaymentId,
            userId,
            DefaultsGuid.DefaultAmount,
            externalPaymentId: StripeTestData.PaymentIntents.TimestampTest);

        var afterCreation = DateTime.UtcNow;

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        // Assert
        var payment = result.Value.Payments?.FirstOrDefault();
        payment.Should().NotBeNull();
        payment!.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        payment.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnAllPaymentStatuses_Correctly()
    {
        // Arrange
        var userId = DefaultsGuid.UserId;

        await CreatePaymentAsync(DefaultsGuid.PaymentId, userId, DefaultsGuid.DefaultAmount, PaymentStatus.Pending, StripeTestData.PaymentIntents.Pending);
        await CreatePaymentAsync(DefaultsGuid.PaymentId2, userId, DefaultsGuid.DefaultAmount, PaymentStatus.Completed, StripeTestData.PaymentIntents.Completed);
        await CreatePaymentAsync(DefaultsGuid.PaymentId3, userId, DefaultsGuid.DefaultAmount, PaymentStatus.Failed, StripeTestData.PaymentIntents.Failed);
        await CreatePaymentAsync(DefaultsGuid.PaymentId4, userId, DefaultsGuid.DefaultAmount, PaymentStatus.Cancelled, StripeTestData.PaymentIntents.Cancelled);

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().NotBeNullOrEmpty();
        var statuses = result.Value.Payments!.Select(p => p.Status).Distinct();
        statuses.Should().Contain(nameof(PaymentStatus.Pending));
        statuses.Should().Contain(nameof(PaymentStatus.Completed));
        statuses.Should().Contain(nameof(PaymentStatus.Failed));
        statuses.Should().Contain(nameof(PaymentStatus.Cancelled));
    }


    [Fact]
    public async Task Query_GetPaymentHistory_Should_HandleLargePageSizes_Correctly()
    {
        // Arrange
        var userId = DefaultsGuid.UserId;
        for (int i = 0; i < 50; i++)
        {
            await CreatePaymentAsync(
                new Guid($"00000000-0000-0000-0001-{i:000000000000}"),
                userId,
                DefaultsGuid.DefaultAmount,
                externalPaymentId: $"pi_test_large_{i}");
        }

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId, 1, 100));
        }

        // Assert
        result.Value.Payments!.Count.Should().BeLessThanOrEqualTo(100);
        result.Value.TotalCount.Should().BeGreaterThanOrEqualTo(50);
    }
}
