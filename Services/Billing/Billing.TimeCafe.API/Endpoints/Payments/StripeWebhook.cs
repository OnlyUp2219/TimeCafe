namespace Billing.TimeCafe.API.Endpoints.Payments;

public class StripeWebhook : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/billing/payments/webhook/stripe", async (
            [FromServices] ISender sender,
            [FromBody] StripeWebhookPayload payload,
            HttpRequest request) =>
        {
            var signature = request.Headers["Stripe-Signature"].ToString();
            var command = new ProcessStripeWebhookCommand(payload, signature);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: _ => Results.Ok());
        })
        .WithTags("Payments")
        .WithName("StripeWebhook")
        .WithSummary("Webhook от Stripe")
        .WithDescription("Обрабатывает события платежей от Stripe")
        .WithOpenApi()
        .AllowAnonymous();
    }
}
