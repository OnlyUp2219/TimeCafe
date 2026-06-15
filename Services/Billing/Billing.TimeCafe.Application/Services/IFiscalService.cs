using Billing.TimeCafe.Domain.Models;
using FluentResults;

namespace Billing.TimeCafe.Application.Services;

public record FiscalReceiptResult(string ReceiptNumber, string? Url);

public interface IFiscalService
{
    Task<Result<FiscalReceiptResult>> RegisterReceiptAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
