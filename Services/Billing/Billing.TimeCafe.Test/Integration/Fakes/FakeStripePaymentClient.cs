namespace Billing.TimeCafe.Test.Integration.Fakes;

public class FakeStripePaymentClient : IStripePaymentClient
{
    public Task<StripeCreatePaymentResponse> CreatePaymentAsync(StripeCreatePaymentRequest request, CancellationToken ct = default)
    {
        var response = new StripeCreatePaymentResponse(
            true,
            $"pi_{request.PaymentId:N}",
            "cs_test",
            "pk_test",
            null);
        return Task.FromResult(response);
    }
}
