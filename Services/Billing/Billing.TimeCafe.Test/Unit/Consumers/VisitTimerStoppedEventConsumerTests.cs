using Billing.TimeCafe.Application.CQRS.Invoices.Commands;
using Billing.TimeCafe.Domain.Models;
using Billing.TimeCafe.Domain.Contracts;
using Billing.TimeCafe.Infrastructure.Consumers;
using BuildingBlocks.Events;
using FluentResults;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DomainPaymentMethod = Billing.TimeCafe.Domain.Enums.PaymentMethod;

namespace Billing.TimeCafe.Test.Unit.Consumers;

public class VisitTimerStoppedEventConsumerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IBalanceRepository> _balanceRepositoryMock;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<ILogger<VisitTimerStoppedEventConsumer>> _loggerMock;
    private readonly Mock<ConsumeContext<VisitTimerStoppedEvent>> _contextMock;

    private readonly VisitTimerStoppedEventConsumer _consumer;

    public VisitTimerStoppedEventConsumerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _balanceRepositoryMock = new Mock<IBalanceRepository>();
        _publisherMock = new Mock<IPublisher>();
        _senderMock = new Mock<ISender>();
        _loggerMock = new Mock<ILogger<VisitTimerStoppedEventConsumer>>();
        _contextMock = new Mock<ConsumeContext<VisitTimerStoppedEvent>>();

        _uowMock.Setup(u => u.Invoices).Returns(_invoiceRepositoryMock.Object);
        _uowMock.Setup(u => u.Balances).Returns(_balanceRepositoryMock.Object);

        _consumer = new VisitTimerStoppedEventConsumer(
            _uowMock.Object,
            _publisherMock.Object,
            _loggerMock.Object,
            _senderMock.Object);
    }

    [Fact]
    public async Task Consume_ShouldCreateInvoice_WhenInvoiceDoesNotExist()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var evt = new VisitTimerStoppedEvent { VisitId = visitId, UserId = userId, Amount = 1500m, StoppedAt = DateTime.UtcNow, PayFromBalance = false };
        _contextMock.Setup(c => c.Message).Returns(evt);
        _contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        _invoiceRepositoryMock.Setup(r => r.GetByVisitIdAsync(visitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _invoiceRepositoryMock.Verify(r => r.CreateAsync(It.Is<Invoice>(i => i.VisitId == visitId && i.UserId == userId && i.TotalAmount == 1500m), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(p => p.Publish(It.IsAny<InvoiceChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(It.IsAny<PayInvoiceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_ShouldAutoPay_WhenPayFromBalanceIsTrueAndBalanceIsSufficient()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var evt = new VisitTimerStoppedEvent { VisitId = visitId, UserId = userId, Amount = 1500m, StoppedAt = DateTime.UtcNow, PayFromBalance = true };
        _contextMock.Setup(c => c.Message).Returns(evt);
        _contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        _invoiceRepositoryMock.Setup(r => r.GetByVisitIdAsync(visitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var balance = Balance.Create(userId);
        balance.Deposit(2000m);
        _balanceRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        _senderMock.Setup(s => s.Send(It.IsAny<PayInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _invoiceRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(It.Is<PayInvoiceCommand>(c => c.Method == DomainPaymentMethod.Online), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldNotAutoPay_WhenPayFromBalanceIsTrueButBalanceIsInsufficient()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var evt = new VisitTimerStoppedEvent { VisitId = visitId, UserId = userId, Amount = 1500m, StoppedAt = DateTime.UtcNow, PayFromBalance = true };
        _contextMock.Setup(c => c.Message).Returns(evt);
        _contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        _invoiceRepositoryMock.Setup(r => r.GetByVisitIdAsync(visitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        var balance = Balance.Create(userId);
        balance.Deposit(1000m); // less than 1500
        _balanceRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        await _consumer.Consume(_contextMock.Object);

        // Assert
        _invoiceRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(It.IsAny<PayInvoiceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
