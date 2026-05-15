namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Queries;

public class GetPaymentHistoryQueryTests : BasePaymentTest
{
    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnEmptyList_WhenNoPayments()
    {
        var userId = DefaultsGuid.UserId;

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnAllPayments_WhenExist()
    {
        var userId = DefaultsGuid.UserId;

        await CreatePaymentAsync(DefaultsGuid.PaymentId, userId, DefaultsGuid.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId2, userId, Defaults.MediumAmount, PaymentStatus.Completed, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId3, userId, Defaults.LargeAmount, PaymentStatus.Failed, null);

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.Payments.Should().AllSatisfy(p => p.Should().NotBeNull());
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_OnlyReturnUserPayments()
    {
        var userId1 = DefaultsGuid.UserId;
        var userId2 = DefaultsGuid.UserId2;

        await CreatePaymentAsync(DefaultsGuid.PaymentId, userId1, DefaultsGuid.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId2, userId1, Defaults.MediumAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId3, userId2, Defaults.LargeAmount, PaymentStatus.Pending, null);

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId1));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().HaveCount(2);
        result.Value.Payments.Should().AllSatisfy(p => p.Should().NotBeNull());
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

        Result<GetPaymentHistoryResponse> page1;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            page1 = await sender.Send(new GetPaymentHistoryQuery(userId, 1, pageSize));
        }

        Result<GetPaymentHistoryResponse> page2;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            page2 = await sender.Send(new GetPaymentHistoryQuery(userId, 2, pageSize));
        }

        page1.IsSuccess.Should().BeTrue();
        page1.Value.Payments.Should().HaveCount(2);
        page1.Value.TotalCount.Should().Be(4);
        page1.Value.TotalPages.Should().Be(2);

        page2.IsSuccess.Should().BeTrue();
        page2.Value.Payments.Should().HaveCount(2);
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

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().HaveCount(1);

        var payment = result.Value.Payments[0];
        payment.PaymentId.Should().Be(paymentId);
        payment.Amount.Should().Be(amount);
        payment.Status.Should().Be(status.ToString());
        payment.ExternalPaymentId.Should().Be(externalId);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_IncludeAllPaymentStatuses()
    {
        var userId = DefaultsGuid.UserId;

        await CreatePaymentAsync(DefaultsGuid.PaymentId, userId, DefaultsGuid.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId2, userId, Defaults.MediumAmount, PaymentStatus.Completed, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId3, userId, Defaults.LargeAmount, PaymentStatus.Failed, null);
        await CreatePaymentAsync(DefaultsGuid.PaymentId4, userId, Defaults.ExtraLargeAmount, PaymentStatus.Cancelled, null);

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().HaveCount(4);
        result.Value.Payments.Should().Contain(p => p.Status == nameof(PaymentStatus.Pending));
        result.Value.Payments.Should().Contain(p => p.Status == nameof(PaymentStatus.Completed));
        result.Value.Payments.Should().Contain(p => p.Status == nameof(PaymentStatus.Failed));
        result.Value.Payments.Should().Contain(p => p.Status == nameof(PaymentStatus.Cancelled));
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_IncludeTimestamps()
    {
        var userId = DefaultsGuid.UserId;
        var paymentId = DefaultsGuid.PaymentId;

        await CreatePaymentAsync(paymentId, userId, DefaultsGuid.SmallAmount, PaymentStatus.Completed, null);

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId));
        }

        result.IsSuccess.Should().BeTrue();
        var payment = result.Value.Payments[0];
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

        Result<GetPaymentHistoryResponse> result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId, 1, 100));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().HaveCount(5);
        result.Value.TotalPages.Should().Be(1);
    }
}
