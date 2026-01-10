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

        await repo.CreateAsync(payment);
        return payment;
    }

    protected async Task<PaymentModel> CreatePendingPaymentAsync(Guid paymentId, Guid userId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Pending);

    protected async Task<PaymentModel> CreateCompletedPaymentAsync(Guid paymentId, Guid userId, string externalPaymentId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Completed, externalPaymentId);

    protected async Task<PaymentModel> CreateFailedPaymentAsync(Guid paymentId, Guid userId)
        => await CreatePaymentAsync(paymentId, userId, Defaults.DefaultAmount, PaymentStatus.Failed);

    protected async Task<PaymentModel> CreateCancelledPaymentAsync(Guid paymentId, Guid userId)
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

    protected async Task<PaymentModel?> GetPaymentByIdAsync(Guid paymentId)
    {
        using var scope = CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        return await repo.GetByIdAsync(paymentId);
    }

    protected async Task<BalanceModel?> GetBalanceByUserIdAsync(Guid userId)
    {
        using var scope = CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        return await repo.GetByUserIdAsync(userId);
    }

    protected async Task<List<PaymentModel>> GetPaymentsByUserIdAsync(Guid userId, int page = 1, int pageSize = 100)
    {
        using var scope = CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        return await repo.GetByUserIdAsync(userId, page, pageSize);
    }

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

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
