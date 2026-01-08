namespace Auth.TimeCafe.Application.CQRS.Sender.Commands;

public record class SendSmsCommand(string AccountSid, string AuthToken, string TwilioPhoneNumber, string PhoneNumber, string Token) : IRequest<PhoneVerificationResult?>;

public class SendSmsCommandValidator : AbstractValidator<SendSmsCommand>
{
    public SendSmsCommandValidator()
    {
        RuleFor(x => x.AccountSid)
            .NotEmpty().WithMessage("AccountSid обязателен");

        RuleFor(x => x.AuthToken)
            .NotEmpty().WithMessage("AuthToken обязателен");

        RuleFor(x => x.TwilioPhoneNumber)
            .NotEmpty().WithMessage("TwilioPhoneNumber обязателен");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона обязателен")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Неверный формат номера телефона");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Токен верификации обязателен");
    }
}

public class SendSmsCommandHendler(ITwilioSender twilioSender, ILogger<SendSmsCommandHendler> logger) : IRequestHandler<SendSmsCommand, PhoneVerificationResult?>
{
    private readonly ITwilioSender _twilioSender = twilioSender;
    private readonly ILogger<SendSmsCommandHendler> _logger = logger;

    public async Task<PhoneVerificationResult?> Handle(SendSmsCommand request, CancellationToken cancellationToken)
    {
        var result = await _twilioSender.SendAsync(
            request.AccountSid,
            request.AuthToken,
            request.TwilioPhoneNumber,
            request.PhoneNumber,
            request.Token);

        if (result == null)
        {
            _logger.LogWarning("Не удалось отправить SMS на номер {PhoneNumber}", request.PhoneNumber);
        }

        return result;
    }
}
