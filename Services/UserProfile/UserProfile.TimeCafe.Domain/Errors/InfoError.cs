namespace UserProfile.TimeCafe.Domain.Errors;

public sealed class InfoNotFoundError : Error
{
    public InfoNotFoundError() : base("Информация не найдена")
    {
        Metadata.Add("ErrorCode", "404");
    }
}
