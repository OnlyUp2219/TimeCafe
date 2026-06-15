using Billing.TimeCafe.Application.Options;
using Billing.TimeCafe.Domain.Models;
using Billing.TimeCafe.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Billing.TimeCafe.Test.Unit.Services;

public class MockFiscalServiceTests
{
    private class TestOptionsSnapshot<T>(T value) : IOptionsSnapshot<T> where T : class
    {
        public T Value { get; } = value;
        public T Get(string? name) => Value;
    }

    [Fact]
    public async Task RegisterReceiptAsync_WhenEnabled_ShouldReturnReceiptNumber()
    {
        // Arrange
        var options = new FiscalOptions { UseMockFiscalService = true };
        var optionsSnapshot = new TestOptionsSnapshot<FiscalOptions>(options);
        
        var service = new MockFiscalService(optionsSnapshot, NullLogger<MockFiscalService>.Instance);
        var invoice = Invoice.Create(Guid.NewGuid(), Guid.NewGuid(), 150m).Value;

        // Act
        var result = await service.RegisterReceiptAsync(invoice, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ReceiptNumber.Should().StartWith($"ФП-{DateTime.UtcNow.Year}-");
        result.Value.Url.Should().BeNull();
    }

    [Fact]
    public async Task RegisterReceiptAsync_WhenDisabled_ShouldReturnError()
    {
        // Arrange
        var options = new FiscalOptions { UseMockFiscalService = false };
        var optionsSnapshot = new TestOptionsSnapshot<FiscalOptions>(options);
        
        var service = new MockFiscalService(optionsSnapshot, NullLogger<MockFiscalService>.Instance);
        var invoice = Invoice.Create(Guid.NewGuid(), Guid.NewGuid(), 150m).Value;

        // Act
        var result = await service.RegisterReceiptAsync(invoice, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Реальный фискальный сервис не настроен");
    }
}
