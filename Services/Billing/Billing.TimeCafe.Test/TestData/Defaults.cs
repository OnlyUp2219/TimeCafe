namespace Billing.TimeCafe.Test.TestData;

public static class Defaults
{
    public static Guid UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static Guid UserId2 => Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static Guid UserId3 => Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static Guid TariffId => Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static Guid PaymentId => Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static Guid PaymentId2 => Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static Guid PaymentId3 => Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static Guid PaymentId4 => Guid.Parse("12121212-1212-1212-1212-121212121212");
    public static Guid PaymentId5 => Guid.Parse("14141414-1414-1414-1414-141414141414");
    public static Guid PaymentId6 => Guid.Parse("15151515-1515-1515-1515-151515151515");
    public static Guid TransactionId => Guid.Parse("13131313-1313-1313-1313-131313131313");

    public static decimal DefaultAmount => 500m;
    public static decimal UpdatedAmount => 1000m;
    public static decimal SmallAmount => 100m;
    public static decimal DebtAmount => 50m;
    public static decimal MediumAmount => 200m;
    public static decimal LargeAmount => 300m;
    public static decimal ExtraLargeAmount => 400m;
    public static decimal MinimumPaymentAmount => 50m;
    public static decimal BelowMinimumAmount => 30m;
    public static decimal PremiumSubscriptionAmount => 750m;
    public static decimal MismatchedStripeAmount => 600m;

    public static string StripePaymentIntentId => StripeTestData.PaymentIntents.Default;
    public static string StripePaymentIntentId2 => StripeTestData.PaymentIntents.Secondary;
    public static string StripeNonExistentPaymentIntentId => StripeTestData.PaymentIntents.NonExistent;
}

public static class InvalidData
{
    public static Guid NonExistentUserId => Guid.Parse("99999999-9999-9999-9999-999999999999");
    public static Guid AnotherNonExistentUserId => Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static Guid NonExistentPaymentId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static Guid EmptyUserId => Guid.Empty;
    public static Guid RandomUserId => Guid.NewGuid();
}

public static class StripeTestData
{
    public static class PaymentIntents
    {
        public static string Default => "pi_test_55555555555555555555555555555555";
        public static string Secondary => "pi_test_66666666666666666666666666666666";
        public static string NonExistent => "pi_test_99999999999999999999999999999999";
        public static string NewExternal => "pi_new_external_id_123456789012345";
        public static string External => "pi_test_external_123";
    }

    public static class Events
    {
        public static string Succeeded => "payment_intent.succeeded";
        public static string Failed => "payment_intent.payment_failed";
        public static string Cancelled => "payment_intent.canceled";
        public static string Refunded => "charge.refunded";
    }

    public static class Statuses
    {
        public static string Refunded => "refunded";
        public static string Succeeded => "succeeded";
        public static string RequiresPaymentMethod => "requires_payment_method";
        public static string Canceled => "canceled";
    }

    public static class Configuration
    {
        public static string WebhookSecret => "whsec_test_secret";
        public static string DefaultReturnUrl => "https://example.com/return";
    }

    public static class Descriptions
    {
        public static string PremiumSubscription => "Premium subscription";
        public static string BalanceReplenishment => "Пополнение баланса";
    }
}
