namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Queries;

public class GetPaymentHistoryQueryWithRealStripeTests : BasePaymentTest
{
    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnPaymentsFromRealStripe_WhenUserHasPayments()
    {
        // Arrange
        var userId = Defaults.UserId;

        await CreatePaymentAsync(
            Defaults.PaymentId,
            userId,
            Defaults.DefaultAmount,
            PaymentStatus.Completed,
            externalPaymentId: StripeTestData.PaymentIntents.RealTest1);

        await CreatePaymentAsync(
            Defaults.PaymentId2,
            userId,
            Defaults.SmallAmount,
            PaymentStatus.Completed,
            externalPaymentId: StripeTestData.PaymentIntents.RealTest2);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString()));
        }

        // Assert
        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Payments.Should().Contain(p => p.ExternalPaymentId == StripeTestData.PaymentIntents.RealTest1);
        result.Payments.Should().Contain(p => p.ExternalPaymentId == StripeTestData.PaymentIntents.RealTest2);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnEmptyList_WhenUserHasNoPayments()
    {
        // Arrange
        var userId = Defaults.UserId2;

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString()));
        }

        // Assert
        result.Success.Should().BeTrue();
        result.Payments.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_FilterByUserIdCorrectly_WhenMultipleUsersHavePayments()
    {
        // Arrange
        var userId1 = Defaults.UserId;
        var userId2 = Defaults.UserId2;

        await CreatePaymentAsync(Defaults.PaymentId, userId1, Defaults.DefaultAmount, externalPaymentId: StripeTestData.PaymentIntents.RealUser1);
        await CreatePaymentAsync(Defaults.PaymentId2, userId2, Defaults.SmallAmount, externalPaymentId: StripeTestData.PaymentIntents.RealUser2);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId1.ToString()));
        }

        // Assert
        result.Success.Should().BeTrue();
        var userPayments = result.Payments!.Where(p => p.ExternalPaymentId == StripeTestData.PaymentIntents.RealUser1);
        userPayments.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Payments.Should().NotContain(p => p.ExternalPaymentId == StripeTestData.PaymentIntents.RealUser2);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_HandlePaginationCorrectly_WhenRequestingDifferentPages()
    {
        // Arrange
        var userId = Defaults.UserId;
        for (int i = 0; i < 15; i++)
        {
            await CreatePaymentAsync(
                new Guid($"00000000-0000-0000-0000-{i:000000000000}"),
                userId,
                Defaults.DefaultAmount + i,
                externalPaymentId: $"pi_test_{i}");
        }

        GetPaymentHistoryResult page1Result;
        GetPaymentHistoryResult page2Result;

        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            page1Result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString(), 1, 5));
            page2Result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString(), 2, 5));
        }

        // Assert
        page1Result.Payments.Should().HaveCount(5);
        page2Result.Payments.Should().HaveCount(5);
        page1Result.Payments.Should().NotIntersectWith(page2Result.Payments);
        page1Result.TotalCount.Should().BeGreaterThanOrEqualTo(15);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_IncludeStripeMetadata_InPaymentDetails()
    {
        // Arrange
        var userId = Defaults.UserId;
        var externalPaymentId = StripeTestData.PaymentIntents.MetadataTest;

        await CreatePaymentAsync(
            Defaults.PaymentId,
            userId,
            Defaults.DefaultAmount,
            PaymentStatus.Completed,
            externalPaymentId: externalPaymentId);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString()));
        }

        // Assert
        var payment = result.Payments?.FirstOrDefault(p => p.ExternalPaymentId == externalPaymentId);
        payment.Should().NotBeNull();
        payment!.ExternalPaymentId.Should().StartWith("pi_");
        payment.Amount.Should().Be(Defaults.DefaultAmount);
        payment.Status.Should().Be(PaymentStatus.Completed.ToString());
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_IncludeTimestamps_ForEachPayment()
    {
        // Arrange
        var userId = Defaults.UserId;
        var beforeCreation = DateTime.UtcNow;

        await CreatePaymentAsync(
            Defaults.PaymentId,
            userId,
            Defaults.DefaultAmount,
            externalPaymentId: StripeTestData.PaymentIntents.TimestampTest);

        var afterCreation = DateTime.UtcNow;

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString()));
        }

        // Assert
        var payment = result.Payments?.FirstOrDefault();
        payment.Should().NotBeNull();
        payment!.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        payment.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnAllPaymentStatuses_Correctly()
    {
        // Arrange
        var userId = Defaults.UserId;

        await CreatePaymentAsync(Defaults.PaymentId, userId, Defaults.DefaultAmount, PaymentStatus.Pending, StripeTestData.PaymentIntents.Pending);
        await CreatePaymentAsync(Defaults.PaymentId2, userId, Defaults.DefaultAmount, PaymentStatus.Completed, StripeTestData.PaymentIntents.Completed);
        await CreatePaymentAsync(Defaults.PaymentId3, userId, Defaults.DefaultAmount, PaymentStatus.Failed, StripeTestData.PaymentIntents.Failed);
        await CreatePaymentAsync(Defaults.PaymentId4, userId, Defaults.DefaultAmount, PaymentStatus.Cancelled, StripeTestData.PaymentIntents.Cancelled);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString()));
        }

        // Assert
        result.Success.Should().BeTrue();
        result.Payments.Should().NotBeNullOrEmpty();
        var statuses = result.Payments!.Select(p => p.Status).Distinct();
        statuses.Should().Contain(PaymentStatus.Pending.ToString());
        statuses.Should().Contain(PaymentStatus.Completed.ToString());
        statuses.Should().Contain(PaymentStatus.Failed.ToString());
        statuses.Should().Contain(PaymentStatus.Cancelled.ToString());
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ValidateInvalidUserId_AndReturnError()
    {
        // Arrange
        GetPaymentHistoryResult result;

        // Act & Assert
        var action = async () =>
        {
            using (var scope = CreateScope())
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                result = await sender.Send(new GetPaymentHistoryQuery("invalid-guid"));
            }
        };

        await action.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ValidateNegativePage_AndReturnError()
    {
        // Arrange & Act & Assert
        var action = async () =>
        {
            using (var scope = CreateScope())
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                await sender.Send(new GetPaymentHistoryQuery(Defaults.UserId.ToString(), 0, 10));
            }
        };

        await action.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ValidateInvalidPageSize_AndReturnError()
    {
        // Arrange & Act & Assert
        var action = async () =>
        {
            using (var scope = CreateScope())
            {
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                await sender.Send(new GetPaymentHistoryQuery(Defaults.UserId.ToString(), 1, 0));
            }
        };

        await action.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_HandleLargePageSizes_Correctly()
    {
        // Arrange
        var userId = Defaults.UserId;
        for (int i = 0; i < 50; i++)
        {
            await CreatePaymentAsync(
                new Guid($"00000000-0000-0000-0001-{i:000000000000}"),
                userId,
                Defaults.DefaultAmount,
                externalPaymentId: $"pi_test_large_{i}");
        }

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString(), 1, 100));
        }

        // Assert
        result.Payments!.Count.Should().BeLessThanOrEqualTo(100);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(50);
    }
}
