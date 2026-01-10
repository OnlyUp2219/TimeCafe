namespace Billing.TimeCafe.Test.Integration.CQRS.Payments.Queries;

public class GetPaymentHistoryQueryTests : BasePaymentTest
{
    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnEmptyList_WhenNoPayments()
    {
        var userId = Defaults.UserId;

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString()));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnAllPayments_WhenExist()
    {
        var userId = Defaults.UserId;

        await CreatePaymentAsync(Defaults.PaymentId, userId, Defaults.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId2, userId, Defaults.MediumAmount, PaymentStatus.Completed, null);
        await CreatePaymentAsync(Defaults.PaymentId3, userId, Defaults.LargeAmount, PaymentStatus.Failed, null);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString()));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Payments.Should().AllSatisfy(p => p.Should().NotBeNull());
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_OnlyReturnUserPayments()
    {
        var userId1 = Defaults.UserId;
        var userId2 = Defaults.UserId2;

        await CreatePaymentAsync(Defaults.PaymentId, userId1, Defaults.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId2, userId1, Defaults.MediumAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId3, userId2, Defaults.LargeAmount, PaymentStatus.Pending, null);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId1.ToString()));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCount(2);
        result.Payments.Should().AllSatisfy(p => p.Should().NotBeNull());
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_PaginateCorrectly()
    {
        var userId = Defaults.UserId;
        var pageSize = 2;

        await CreatePaymentAsync(Defaults.PaymentId, userId, Defaults.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId2, userId, Defaults.MediumAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId3, userId, Defaults.LargeAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId4, userId, Defaults.ExtraLargeAmount, PaymentStatus.Pending, null);

        GetPaymentHistoryResult page1;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            page1 = await sender.Send(new GetPaymentHistoryQuery(userId.ToString(), 1, pageSize));
        }

        GetPaymentHistoryResult page2;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            page2 = await sender.Send(new GetPaymentHistoryQuery(userId.ToString(), 2, pageSize));
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
        var userId = Defaults.UserId;
        var paymentId = Defaults.PaymentId;
        var amount = 555m;
        const PaymentStatus status = PaymentStatus.Completed;
        const string externalId = "pi_test_external_123";

        await CreatePaymentAsync(paymentId, userId, amount, status, externalId);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString()));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCount(1);

        var payment = result.Payments!.First();
        payment.PaymentId.Should().Be(paymentId);
        payment.Amount.Should().Be(amount);
        payment.Status.Should().Be(status.ToString());
        payment.ExternalPaymentId.Should().Be(externalId);
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnError_WhenUserIdInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetPaymentHistoryQuery("invalid-guid"));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnError_WhenUserIdEmpty()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetPaymentHistoryQuery(""));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnError_WhenPageInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetPaymentHistoryQuery(Defaults.UserId.ToString(), 0, 20));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnError_WhenPageSizeInvalid()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetPaymentHistoryQuery(Defaults.UserId.ToString(), 1, 0));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_ReturnError_WhenPageSizeExceedsMax()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var action = async () => await sender.Send(new GetPaymentHistoryQuery(Defaults.UserId.ToString(), 1, 101));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_IncludeAllPaymentStatuses()
    {
        var userId = Defaults.UserId;

        await CreatePaymentAsync(Defaults.PaymentId, userId, Defaults.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId2, userId, Defaults.MediumAmount, PaymentStatus.Completed, null);
        await CreatePaymentAsync(Defaults.PaymentId3, userId, Defaults.LargeAmount, PaymentStatus.Failed, null);
        await CreatePaymentAsync(Defaults.PaymentId4, userId, Defaults.ExtraLargeAmount, PaymentStatus.Cancelled, null);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString()));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCount(4);
        result.Payments.Should().Contain(p => p.Status == PaymentStatus.Pending.ToString());
        result.Payments.Should().Contain(p => p.Status == PaymentStatus.Completed.ToString());
        result.Payments.Should().Contain(p => p.Status == PaymentStatus.Failed.ToString());
        result.Payments.Should().Contain(p => p.Status == PaymentStatus.Cancelled.ToString());
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_IncludeTimestamps()
    {
        var userId = Defaults.UserId;
        var paymentId = Defaults.PaymentId;

        await CreatePaymentAsync(paymentId, userId, Defaults.SmallAmount, PaymentStatus.Completed, null);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString()));
        }

        result.Success.Should().BeTrue();
        var payment = result.Payments!.First();
        payment.CreatedAt.Should().NotBe(default(DateTimeOffset));
    }

    [Fact]
    public async Task Query_GetPaymentHistory_Should_HandleLargePageSize()
    {
        var userId = Defaults.UserId;

        await CreatePaymentAsync(Defaults.PaymentId, userId, Defaults.SmallAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId2, userId, Defaults.MediumAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId3, userId, Defaults.LargeAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId4, userId, Defaults.ExtraLargeAmount, PaymentStatus.Pending, null);
        await CreatePaymentAsync(Defaults.PaymentId5, userId, Defaults.DefaultAmount, PaymentStatus.Pending, null);

        GetPaymentHistoryResult result;
        using (var scope = CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(new GetPaymentHistoryQuery(userId.ToString(), 1, 100));
        }

        result.Success.Should().BeTrue();
        result.Payments.Should().HaveCount(5);
        result.TotalPages.Should().Be(1);
    }
}
