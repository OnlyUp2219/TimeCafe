namespace TimeCafe.Core.Contracts.Services.Clients;

public interface IClientValidation
{
    Task<bool> ValidateAccessCardNumberAsync(string accessCardNumber);
    Task<bool> ValidateEmailAsync(string email);
    Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
    Task<bool> ValidatePhoneNumberFormatAsync(string phoneNumber);
}