namespace Auth.TimeCafe.API.DTOs;

public record SavePhoneRequest(string PhoneNumber);

public class SavePhoneRequestExample : IExamplesProvider<SavePhoneRequest>
{
    public SavePhoneRequest GetExamples() =>
        new("+375291234567");
}
