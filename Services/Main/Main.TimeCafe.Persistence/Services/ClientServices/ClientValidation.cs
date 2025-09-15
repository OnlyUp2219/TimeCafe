using System.Text.RegularExpressions;
using TimeCafe.Core.Contracts.Services.Clients;

namespace TimeCafe.Persistence.Services.ClientServices;

public class ClientValidation : IClientValidation
{
    private readonly TimeCafeContext _context;

    public ClientValidation(TimeCafeContext context)
    {
        _context = context;
    }

    public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        if (await _context.Clients.AnyAsync(c => c.PhoneNumber == phoneNumber))
            return false;

        return await ValidatePhoneNumberFormatAsync(phoneNumber);
    }

    public async Task<bool> ValidatePhoneNumberFormatAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var phoneRegex = new Regex(@"^\+375 \([0-9]{2}\) [0-9]{3} [0-9]{4}$|^\+375-[0-9]{2}-[0-9]{7}$");
        return phoneRegex.IsMatch(phoneNumber);
    }

    public async Task<bool> ValidateEmailAsync(string email)
    {
        var atCount = email.Count(c => c == '@');
        if (atCount != 1)
            return false;

        return !await _context.Clients.AnyAsync(c => c.Email == email);
    }

    public async Task<bool> ValidateAccessCardNumberAsync(string accessCardNumber)
    {
        if (string.IsNullOrWhiteSpace(accessCardNumber))
            return true;

        if (accessCardNumber.Length != 20)
            return false;

        return !await _context.Clients.AnyAsync(c => c.AccessCardNumber == accessCardNumber);
    }
}