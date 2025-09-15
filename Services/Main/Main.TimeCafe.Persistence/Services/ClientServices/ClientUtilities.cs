using TimeCafe.Core.Contracts.Services.Clients;
using TimeCafe.Core.Enums;

namespace TimeCafe.Persistence.Services.ClientServices;

public class ClientUtilities : IClientUtilities
{
    private readonly Dictionary<string, string> _phoneConfirmationCodes = new();
    private readonly Dictionary<int, bool> _confirmedPhones = new();
    private readonly TimeCafeContext _context;
    private readonly IClientValidation _clientValidation;


    public ClientUtilities(TimeCafeContext context, IClientValidation clientValidation)
    {
        _context = context;
        _clientValidation = clientValidation;
    }

    public async Task<bool> SendPhoneConfirmationCodeAsync(string phoneNumber)
    {
        if (!await _clientValidation.ValidatePhoneNumberAsync(phoneNumber))
            return false;

        var code = new Random().Next(100000, 999999).ToString();
        _phoneConfirmationCodes[phoneNumber] = code;
        return true;
    }

    public async Task<bool> VerifyPhoneConfirmationCodeAsync(string phoneNumber, string code)
    {
        if (!_phoneConfirmationCodes.TryGetValue(phoneNumber, out var storedCode))
            return false;

        if (storedCode != code)
            return false;

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        if (client != null)
        {
            _confirmedPhones[client.ClientId] = true;
            client.StatusId = (int)EClientStatusType.Active;
            await _context.SaveChangesAsync();
            return true;
        }

        _phoneConfirmationCodes.Remove(phoneNumber);
        return true;
    }

    public async Task<string> GenerateAccessCardNumberAsync()
    {
        var random = new Random();
        var cardNumber = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 20)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        while (await _context.Clients.AnyAsync(c => c.AccessCardNumber == cardNumber))
        {
            cardNumber = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 20)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        return cardNumber;
    }
}