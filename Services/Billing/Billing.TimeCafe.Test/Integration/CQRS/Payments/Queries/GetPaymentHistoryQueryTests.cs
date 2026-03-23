namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Queries;

public class GetPaymentHistoryQueryTests : BasePaymentTest
{
    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnEmptyList_WhenNoPayments()
    {
        var userId = DefaultsGuid.UserId;

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnAllPayments_WhenExist()
    {
        var userId = DefaultsGuid.UserId;

        await CreatePaymentAsync(DefaultsGuid.PaymentId, userId, DefaultsGuid.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId2, userId, Defaults.MediumAmount, PaymentStatus.Completed, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId3, userId, Defaults.LargeAmount, PaymentStatus.Failed, null);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Payments.Should().AllSatisfy(p => p.Should().NotBeNull());
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_OnlyReturnUserPayments()
    {
        var userId1 = DefaultsGuid.UserId;
        var userId2 = DefaultsGuid.UserId2;

        await CreatePaymentAsync(DefaultsGuid.PaymentId, userId1, DefaultsGuid.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId2, userId1, Defaults.MediumAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId3, userId2, Defaults.LargeAmount, PaymentStatus.Pending, null);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId1));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCount(2);
        result.Payments.Should().AllSatisfy(p => p.Should().NotBeNull());
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_PaginateCorrectly()
    {
        var userId = DefaultsGuid.UserId;
        const int pageSize = 2;

        await CreatePaymentAsync(DefaultsGuid.PaymentId, userId, DefaultsGuid.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId2, userId, Defaults.MediumAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId3, userId, Defaults.LargeAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId4, userId, Defaults.ExtraLargeAmount, PaymentStatus.Pending, null);

        GetPaymentHistoryResult page1;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            page1 = await sender.Send(new GetPaymentHistoryQuery(userId, 1, pageSize));
        }

        GetPaymentHistoryResult page2;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            page2 = await sender.Send(new GetPaymentHistoryQuery(userId, 2, pageSize));
        }

        page1.Success.Should().BeTrue();
        page1.Payments.Should().HaveCount(2);
        page1.TotalCount.Should().Be(4);
        page1.TotalPages.Should().Be(2);

        page2.Success.Should().BeTrue();
        page2.Payments.Should().HaveCount(2);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnCorrectPaymentDetails()
    {
        var userId = DefaultsGuid.UserId;
        var paymentId = DefaultsGuid.PaymentId;
        const decimal amount = 555m;
        const PaymentStatus status = PaymentStatus.Completed;
        const string externalId = "pi_test_external_123";

        await CreatePaymentAsync(paymentId, userId, amount, status, externalId);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCount(1);

        var payment = result.Payments![0];
        payment.PaymentId.Should().Be(paymentId);
        payment.Amount.Should().Be(amount);
        payment.Status.Should().Be(status.ToString());
        payment.ExternalPaymentId.Should().Be(externalId);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnError_WhenUserIdEmpty()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetPaymentHistoryQuery(Guid.Empty));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnError_WhenPageInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetPaymentHistoryQuery(DefaultsGuid.UserId, 0, 20));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnError_WhenPageSizeInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetPaymentHistoryQuery(DefaultsGuid.UserId, 1, 0));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnError_WhenPageSizeExceedsMax()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetPaymentHistoryQuery(DefaultsGuid.UserId, 1, 101));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_IncludeAllPaymentStatuses()
    {
        var userId = DefaultsGuid.UserId;

        await CreatePaymentAsync(DefaultsGuid.PaymentId, userId, DefaultsGuid.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId2, userId, Defaults.MediumAmount, PaymentStatus.Completed, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId3, userId, Defaults.LargeAmount, PaymentStatus.Failed, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId4, userId, Defaults.ExtraLargeAmount, PaymentStatus.Cancelled, null);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCount(4);
        result.Payments.Should().Contain(p => p.Status == nameof(PaymentStatus.Pending));
        result.Payments.Should().Contain(p => p.Status == nameof(PaymentStatus.Completed));
        result.Payments.Should().Contain(p => p.Status == nameof(PaymentStatus.Failed));
        result.Payments.Should().Contain(p => p.Status == nameof(PaymentStatus.Cancelled));
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_IncludeTimestamps()
    {
        var userId = DefaultsGuid.UserId;
        var paymentId = DefaultsGuid.PaymentId;

        await CreatePaymentAsync(paymentId, userId, DefaultsGuid.SmallAmount, PaymentStatus.Completed, null);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        result.Success.Should().BeTrue();
        var payment = result.Payments![0];
        payment.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_HandleLargePageSize()
    {
        var userId = DefaultsGuid.UserId;

        await CreatePaymentAsync(DefaultsGuid.PaymentId, userId, DefaultsGuid.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId2, userId, Defaults.MediumAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId3, userId, Defaults.LargeAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId4, userId, Defaults.ExtraLargeAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId5, userId, DefaultsGuid.DefaultAmount, PaymentStatus.Pending, null);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId, 1, 100));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCount(5);
        result.TotalPages.Should().Be(1);
    }
}
