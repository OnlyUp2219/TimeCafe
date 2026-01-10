namespace Billing.TimeCafe.Test.TestData;

public static class Defaults
{
    public static string UserId => "11111111-1111-1111-1111-111111111111";
    public static string UserId2 => "22222222-2222-2222-2222-222222222222";
    public static string UserId3 => "33333333-3333-3333-3333-333333333333";
    public static string TariffId => "44444444-4444-4444-4444-444444444444";
    public static string PaymentId => "55555555-5555-5555-5555-555555555555";
    public static string PaymentId2 => "66666666-6666-6666-6666-666666666666";
    public static string PaymentId3 => "77777777-7777-7777-7777-777777777777";
    public static string PaymentId4 => "12121212-1212-1212-1212-121212121212";
    public static string PaymentId5 => "14141414-1414-1414-1414-141414141414";
    public static string PaymentId6 => "15151515-1515-1515-1515-151515151515";
    public static string TransactionId => "13131313-1313-1313-1313-131313131313";

    public static decimal DefaultAmount => 500m;
    public static decimal UpdatedAmount => 1000m;
    public static decimal SmallAmount => 100m;
    public static decimal MediumAmount => 200m;
    public static decimal LargeAmount => 300m;
    public static decimal ExtraLargeAmount => 400m;
    public static decimal BelowMinimumAmount => 30m;
    public static decimal PremiumSubscriptionAmount => 750m;

    public static string StripePaymentIntentId => StripeTestData.PaymentIntents.Default;
    public static string StripePaymentIntentId2 => StripeTestData.PaymentIntents.Secondary;
    public static string StripeNonExistentPaymentIntentId => StripeTestData.PaymentIntents.NonExistent;
}

public static class InvalidData
{
    public static string NonExistentUserId => "99999999-9999-9999-9999-999999999999";
    public static string AnotherNonExistentUserId => "88888888-8888-8888-8888-888888888888";
    public static string NonExistentPaymentId => "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    public static string EmptyUserId => Guid.Empty.ToString();
    public static string RandomUserId => Guid.NewGuid().ToString();
}

public static class DefaultsGuid
{
    public static Guid UserId => Guid.Parse(Defaults.UserId);
    public static Guid UserId2 => Guid.Parse(Defaults.UserId2);
    public static Guid UserId3 => Guid.Parse(Defaults.UserId3);
    public static Guid TariffId => Guid.Parse(Defaults.TariffId);
    public static Guid PaymentId => Guid.Parse(Defaults.PaymentId);
    public static Guid PaymentId2 => Guid.Parse(Defaults.PaymentId2);
    public static Guid PaymentId3 => Guid.Parse(Defaults.PaymentId3);
    public static Guid PaymentId4 => Guid.Parse(Defaults.PaymentId4);
    public static Guid PaymentId5 => Guid.Parse(Defaults.PaymentId5);
    public static Guid PaymentId6 => Guid.Parse(Defaults.PaymentId6);
    public static Guid TransactionId => Guid.Parse(Defaults.TransactionId);

    public static decimal DefaultAmount => 500m;
    public static decimal UpdatedAmount => 1000m;
    public static decimal SmallAmount => 100m;
    public static decimal DebtAmount => 50m;
}

public static class InvalidDataGuid
{
    public static Guid NonExistentUserId => Guid.Parse(InvalidData.NonExistentUserId);
    public static Guid AnotherNonExistentUserId => Guid.Parse(InvalidData.AnotherNonExistentUserId);
    public static Guid NonExistentPaymentId => Guid.Parse(InvalidData.NonExistentPaymentId);
    public static Guid EmptyUserId => Guid.Empty;
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

        public static string RealTest1 => "pi_test_real_1";
        public static string RealTest2 => "pi_test_real_2";
        public static string RealUser1 => "pi_real_user1";
        public static string RealUser2 => "pi_real_user2";
        public static string MetadataTest => "pi_test_metadata_123";
        public static string TimestampTest => "pi_test_timestamp";
        public static string Pending => "pi_pending";
        public static string Completed => "pi_completed";
        public static string Failed => "pi_failed";
        public static string Cancelled => "pi_cancelled";
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
