using Billing.TimeCafe.Application.Options;
using Billing.TimeCafe.Application.Services;
using Billing.TimeCafe.Domain.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Billing.TimeCafe.Infrastructure.Services;

public class MockFiscalService(
    IOptionsSnapshot<FiscalOptions> options,
    ILogger<MockFiscalService> logger) : IFiscalService
{
    private readonly FiscalOptions _options = options.Value;
    private readonly ILogger<MockFiscalService> _logger = logger;

    public Task<Result<FiscalReceiptResult>> RegisterReceiptAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        if (!_options.UseMockFiscalService)
        {
            _logger.LogError("Попытка использования MockFiscalService при выключенном UseMockFiscalService");
            return Task.FromResult(Result.Fail<FiscalReceiptResult>(new Error("Реальный фискальный сервис не настроен")));
        }

        _logger.LogInformation("Генерация мокового фискального чека для инвойса {InvoiceId}", invoice.InvoiceId);

        var randomId = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var receiptNumber = $"ФП-{DateTime.UtcNow.Year}-{randomId}";

        return Task.FromResult(Result.Ok(new FiscalReceiptResult(receiptNumber, null)));
    }
}
