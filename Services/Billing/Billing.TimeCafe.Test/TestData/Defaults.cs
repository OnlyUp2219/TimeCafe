namespace Billing.TimeCafe.Test.TestData;

public static class Defaults
{
    public static Guid UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static Guid UserId2 => Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static Guid UserId3 => Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static Guid TariffId => Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static decimal DefaultAmount => 500m;
    public static decimal UpdatedAmount => 1000m;
    public static decimal SmallAmount => 100m;
    public static decimal DebtAmount => 50m;
}

public static class InvalidData
{
    public static Guid NonExistentUserId => Guid.Parse("99999999-9999-9999-9999-999999999999");
    public static Guid AnotherNonExistentUserId => Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static Guid EmptyUserId => Guid.Empty;
    public static Guid RandomUserId => Guid.NewGuid();
}
