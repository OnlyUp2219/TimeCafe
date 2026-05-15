namespace Billing.TimeCafe.Domain.Errors;

public sealed class DatabaseUpdateError : Error
{
    public DatabaseUpdateError(string message) : base(message)
    {
        Metadata.Add("ErrorCode", "500");
    }
}
