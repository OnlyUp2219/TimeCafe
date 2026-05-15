namespace UserProfile.TimeCafe.Domain.Errors;

public sealed class PhotoNotFoundError : Error
{
    public PhotoNotFoundError() : base("Фотография не найдена")
    {
        Metadata.Add("ErrorCode", "404");
    }
}
