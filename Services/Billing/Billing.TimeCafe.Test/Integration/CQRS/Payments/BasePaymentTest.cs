namespace Billing.TimeCafe.Test.Integration.CQRS.Payments;

public abstract class BasePaymentTest : IDisposable
{
    protected HttpClient Client { get; }
    protected IntegrationApiFactory Factory { get; }

    protected BasePaymentTest()
    {
        Factory = new IntegrationApiFactory();
        Client = Factory.CreateClient();
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    protected async Task<PaymentModel> CreatePaymentAsync(
        Guid paymentId,
        Guid userId,
        decimal amount,
        PaymentStatus status = PaymentStatus.Pending,
        string? externalPaymentId = null)
    {
        using var scope = CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        var payment = new PaymentModel
        {
            PaymentId = paymentId,
            UserId = userId,
            Amount = amount,
            PaymentMethod = PaymentMethod.Online,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            ExternalPaymentId = externalPaymentId
        };

        await repo.CreateAsync(payment, CancellationToken.None);
        return payment;
    }

    protected async Task<PaymentModel> CreatePaymentAsync(
        string paymentId,
        string userId,
        decimal amount,
        PaymentStatus status = PaymentStatus.Pending,
        string? externalPaymentId = null)
        => await CreatePaymentAsync(
            Guid.Parse(paymentId),
            Guid.Parse(userId),
            amount,
            status,
            externalPaymentId);

    protected async Task<PaymentModel> CreatePaymentAsync(
        string paymentId,
        Guid userId,
        decimal amount,
        PaymentStatus status = PaymentStatus.Pending,
        string? externalPaymentId = null)
        => await CreatePaymentAsync(
            Guid.Parse(paymentId),
            userId,
            amount,
            status,
            externalPaymentId);

    protected async Task<PaymentModel> CreatePaymentAsync(
        Guid paymentId,
        string userId,
        decimal amount,
        PaymentStatus status = PaymentStatus.Pending,
        string? externalPaymentId = null)
        => await CreatePaymentAsync(
            paymentId,
            Guid.Parse(userId),
            amount,
            status,
            externalPaymentId);

    protected async Task<PaymentModel> CreatePendingPaymentAsync(Guid paymentId, Guid userId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Pending);

    protected async Task<PaymentModel> CreateCompletedPaymentAsync(Guid paymentId, Guid userId, string externalPaymentId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Completed, externalPaymentId);

    protected async Task<PaymentModel> CreateFailedPaymentAsync(Guid paymentId, Guid userId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Failed);

    protected async Task<PaymentModel> CreateCancelledPaymentAsync(Guid paymentId, Guid userId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Cancelled);

    protected async Task<PaymentModel> CreatePendingPaymentAsync(string paymentId, string userId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Pending);

    protected async Task<PaymentModel> CreateCompletedPaymentAsync(string paymentId, string userId, string externalPaymentId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Completed, externalPaymentId);

    protected async Task<PaymentModel> CreateFailedPaymentAsync(string paymentId, string userId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Failed);

    protected async Task<PaymentModel> CreateCancelledPaymentAsync(string paymentId, string userId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Cancelled);
    protected async Task<BalanceModel> CreateBalanceAsync(Guid userId, decimal balance = 0m)
    {
        using var scope = CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();

        var balanceModel = new BalanceModel(userId)
        {
            CurrentBalance = balance,
            TotalDeposited = balance,
            Debt = 0m
        };

        await repo.CreateAsync(balanceModel);
        return balanceModel;
    }

    protected async Task<BalanceModel> CreateBalanceAsync(string userId, decimal balance = 0m)
        => await CreateBalanceAsync(Guid.Parse(userId), balance);

    protected async Task<PaymentModel?> GetPaymentByIdAsync(Guid paymentId)
    {
        using var scope = CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        return await repo.GetByIdAsync(paymentId, CancellationToken.None);
    }

    protected async Task<PaymentModel?> GetPaymentByIdAsync(string paymentId)
        => await GetPaymentByIdAsync(Guid.Parse(paymentId));

    protected async Task<BalanceModel?> GetBalanceByUserIdAsync(Guid userId)
    {
        using var scope = CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        return await repo.GetByUserIdAsync(userId, CancellationToken.None);
    }

    protected async Task<BalanceModel?> GetBalanceByUserIdAsync(string userId)
        => await GetBalanceByUserIdAsync(Guid.Parse(userId));

    protected async Task<List<PaymentModel>> GetPaymentsByUserIdAsync(Guid userId, int page = 1, int pageSize = 100)
    {
        using var scope = CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        return await repo.GetByUserIdAsync(userId, page, pageSize);
    }

    protected async Task<List<PaymentModel>> GetPaymentsByUserIdAsync(string userId, int page = 1, int pageSize = 100)
        => await GetPaymentsByUserIdAsync(Guid.Parse(userId), page, pageSize);

    protected StripeWebhookPayload CreateStripeSuccessWebhook(
        string paymentIntentId,
        decimal amountInCents,
        long? createdAt = null)
    {
        return new StripeWebhookPayload
        {
            Type = "payment_intent.succeeded",
            Data = new StripeWebhookData
            {
                Object = new StripePaymentIntentObject
                {
                    Id = paymentIntentId,
                    Amount = (long)amountInCents,
                    Status = "succeeded",
                    Created = createdAt ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            }
        };
    }

    protected StripeWebhookPayload CreateStripeFailedWebhook(
        string paymentIntentId,
        long? createdAt = null)
    {
        return new StripeWebhookPayload
        {
            Type = "payment_intent.payment_failed",
            Data = new StripeWebhookData
            {
                Object = new StripePaymentIntentObject
                {
                    Id = paymentIntentId,
                    Amount = 50000,
                    Status = "requires_payment_method",
                    Created = createdAt ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            }
        };
    }

    protected StripeWebhookPayload CreateStripeCancelledWebhook(
        string paymentIntentId,
        long? createdAt = null)
    {
        return new StripeWebhookPayload
        {
            Type = "payment_intent.canceled",
            Data = new StripeWebhookData
            {
                Object = new StripePaymentIntentObject
                {
                    Id = paymentIntentId,
                    Amount = 50000,
                    Status = "canceled",
                    Created = createdAt ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            }
        };
    }

    protected StripeWebhookPayload CreateStripeCheckoutCompletedWebhook(
        string sessionId,
        decimal amountInCents,
        string? paymentIntentId = null,
        long? createdAt = null)
    {
        return new StripeWebhookPayload
        {
            Type = "checkout.session.completed",
            Data = new StripeWebhookData
            {
                Object = new StripePaymentIntentObject
                {
                    Id = sessionId,
                    AmountTotal = (long)amountInCents,
                    PaymentIntentId = paymentIntentId,
                    Status = "complete",
                    Created = createdAt ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            }
        };
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
