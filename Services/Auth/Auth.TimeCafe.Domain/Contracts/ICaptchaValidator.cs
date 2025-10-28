namespace Auth.TimeCafe.Domain.Contracts;

public interface ICaptchaValidator
{
    Task<bool> ValidateAsync(string? token);
}
