namespace Billing.TimeCafe.API.Endpoints.Payments;

public record DebugStripeWebhookRequest(
    /// <example>checkout.session.completed</example>
    string EventType,
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    Guid UserId,
    /// <example>550e8400-e29b-41d4-a716-446655440001</example>
    Guid PaymentId,
    /// <example>pi_test_123456</example>
    string ExternalPaymentId,
    /// <example>150.00</example>
    decimal Amount);

public class StripeWebhook : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/webhook/stripe", async (
            [FromServices] ISender sender,
            [FromBody] StripeWebhookPayload payload,
            HttpRequest request) =>
        {
            var signature = request.Headers["Stripe-Signature"].ToString();
            var command = new ProcessStripeWebhookCommand(payload, signature);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: () => TypedResults.Ok());
        })
        .WithTags("Payments")
        .WithName("StripeWebhook")
        .WithSummary("Webhook от Stripe")
        .WithDescription("Обрабатывает события платежей от Stripe")
        .Produces(200)
        .AllowAnonymous();

        app.MapPost("/stripe/debug-webhook", async (
            [FromServices] ISender sender,
            [FromServices] IUnitOfWork uow,
            [FromServices] HybridCache cache,
            [FromBody] DebugStripeWebhookRequest request,
            CancellationToken cancellationToken) =>
        {
            var payment = await uow.Payments.GetByIdAsync(request.PaymentId, cancellationToken);
            if (payment == null)
            {
                var newPayment = Payment.Create(request.UserId, request.Amount, PaymentMethod.Online).Value;
                newPayment.PaymentId = request.PaymentId;
                newPayment.ExternalPaymentId = $"cs_test_{request.ExternalPaymentId}";
                await uow.Payments.CreateAsync(newPayment, cancellationToken);
                await uow.SaveChangesAsync(cancellationToken);

                await cache.RemoveByTagAsync(CacheTags.Payment(request.PaymentId), cancellationToken);
                await cache.RemoveByTagAsync(CacheTags.Payments, cancellationToken);
            }

            var payload = new StripeWebhookPayload
            {
                Type = request.EventType,
                Data = new StripeWebhookData
                {
                    Object = new StripePaymentIntentObject
                    {
                        Id = $"cs_test_{request.ExternalPaymentId}",
                        AmountTotal = (long)(request.Amount * 100),
                        PaymentIntentId = request.ExternalPaymentId,
                        Status = request.EventType switch
                        {
                            "checkout.session.completed" => "succeeded",
                            "checkout.session.async_payment_failed" => "failed",
                            "checkout.session.expired" => "expired",
                            _ => "unknown"
                        },
                        Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        Metadata = new Dictionary<string, string>
                        {
                            ["paymentId"] = request.PaymentId.ToString(),
                            ["userId"] = request.UserId.ToString()
                        }
                    }
                }
            };

            var command = new ProcessStripeWebhookCommand(payload, null, BypassSignature: true);
            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                return result.ToHttpResult(onSuccess: () => TypedResults.Ok());
            }

            await cache.RemoveByTagAsync(CacheTags.Payment(request.PaymentId), cancellationToken);
            await cache.RemoveByTagAsync(CacheTags.Payments, cancellationToken);
            await cache.RemoveByTagAsync(CacheTags.PaymentByUser(request.UserId), cancellationToken);
            await cache.RemoveByTagAsync(CacheTags.Balances, cancellationToken);
            await cache.RemoveByTagAsync(CacheTags.Balance(request.UserId), cancellationToken);

            var updatedPayment = await uow.Payments.GetByIdAsync(request.PaymentId, cancellationToken);
            var updatedBalance = await uow.Balances.GetByIdAsync(request.UserId, cancellationToken);
            var transactions = await uow.Transactions.GetBySourceAsync(TransactionSource.Payment, request.PaymentId, cancellationToken);
            var transaction = transactions.FirstOrDefault();

            return TypedResults.Ok(new
            {
                message = "Webhook simulated successfully",
                payment = updatedPayment != null ? new
                {
                    paymentId = updatedPayment.PaymentId,
                    userId = updatedPayment.UserId,
                    amount = updatedPayment.Amount,
                    status = updatedPayment.Status.ToString(),
                    externalPaymentId = updatedPayment.ExternalPaymentId,
                    transactionId = updatedPayment.TransactionId,
                    errorMessage = updatedPayment.ErrorMessage,
                    completedAt = updatedPayment.CompletedAt
                } : null,
                balance = updatedBalance != null ? new
                {
                    userId = updatedBalance.UserId,
                    currentBalance = updatedBalance.CurrentBalance,
                    debt = updatedBalance.Debt,
                    totalDeposited = updatedBalance.TotalDeposited,
                    totalSpent = updatedBalance.TotalSpent,
                    lastUpdated = updatedBalance.LastUpdated
                } : null,
                transaction = transaction != null ? new
                {
                    transactionId = transaction.TransactionId,
                    userId = transaction.UserId,
                    amount = transaction.Amount,
                    type = transaction.Type.ToString(),
                    source = transaction.Source.ToString(),
                    sourceId = transaction.SourceId,
                    balanceAfter = transaction.BalanceAfter,
                    comment = transaction.Comment,
                    createdAt = transaction.CreatedAt
                } : null
            });
        })
        .WithTags("Payments")
        .WithName("DebugStripeWebhook")
        .WithSummary("Имитация webhook от Stripe для отладки")
        .WithDescription("Позволяет симулировать события Stripe в процессе локальной разработки и отладки")
        .Produces(200)
        .AllowAnonymous();
    }
}
