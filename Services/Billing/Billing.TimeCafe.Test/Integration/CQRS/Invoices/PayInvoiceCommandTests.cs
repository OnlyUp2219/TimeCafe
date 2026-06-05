namespace Billing.TimeCafe.Test.Integration.CQRS.Invoices;

public class PayInvoiceCommandTests : BasePaymentTest
{
    [Fact]
    public async Task Handle_Should_PayInvoiceWithBalance_WhenUserHasEnoughFunds()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<Billing.TimeCafe.Infrastructure.Data.ApplicationDbContext>();

        var userId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var amount = 150m;

        await CreateBalanceAsync(userId, 500m);

        var invoice = InvoiceModel.Create(userId, visitId, amount).Value;
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var command = new PayInvoiceCommand(invoice.InvoiceId, PaymentMethod.Online);
        var result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updatedInvoice = await db.Invoices.FindAsync(invoice.InvoiceId);
        updatedInvoice.Should().NotBeNull();
        updatedInvoice!.Status.Should().Be(InvoiceStatus.Paid);
        updatedInvoice.PaymentMethod.Should().Be(PaymentMethod.Online);
        updatedInvoice.PaidAt.Should().NotBeNull();

        var balance = await db.Balances.FindAsync(userId);
        balance.Should().NotBeNull();
        balance!.CurrentBalance.Should().Be(350m);
    }

    [Fact]
    public async Task Handle_Should_Fail_WhenUserHasInsufficientFunds()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<Billing.TimeCafe.Infrastructure.Data.ApplicationDbContext>();

        var userId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var amount = 1000m;

        await CreateBalanceAsync(userId, 200m);

        var invoice = InvoiceModel.Create(userId, visitId, amount).Value;
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var command = new PayInvoiceCommand(invoice.InvoiceId, PaymentMethod.Online);
        var result = await sender.Send(command);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Недостаточно средств");

        var updatedInvoice = await db.Invoices.FindAsync(invoice.InvoiceId);
        updatedInvoice!.Status.Should().Be(InvoiceStatus.Pending);
    }

    [Fact]
    public async Task Handle_Should_PayInvoiceWithCash_AndCreateCompletedPayment()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<Billing.TimeCafe.Infrastructure.Data.ApplicationDbContext>();

        var userId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var amount = 300m;

        var invoice = InvoiceModel.Create(userId, visitId, amount).Value;
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var command = new PayInvoiceCommand(invoice.InvoiceId, PaymentMethod.Cash);
        var result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updatedInvoice = await db.Invoices.FindAsync(invoice.InvoiceId);
        updatedInvoice!.Status.Should().Be(InvoiceStatus.Paid);
        updatedInvoice.PaymentMethod.Should().Be(PaymentMethod.Cash);

        var payments = await db.Payments.Where(p => p.UserId == userId).ToListAsync();
        payments.Should().NotBeEmpty();
        payments[0].Status.Should().Be(PaymentStatus.Completed);
        payments[0].Amount.Should().Be(amount);
        payments[0].PaymentMethod.Should().Be(PaymentMethod.Cash);
    }

    [Fact]
    public async Task Handle_Should_PayInvoiceWithCard_AndCreateCompletedPayment()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<Billing.TimeCafe.Infrastructure.Data.ApplicationDbContext>();

        var userId = Guid.NewGuid();
        var visitId = Guid.NewGuid();
        var amount = 400m;

        var invoice = InvoiceModel.Create(userId, visitId, amount).Value;
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var command = new PayInvoiceCommand(invoice.InvoiceId, PaymentMethod.Card);
        var result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updatedInvoice = await db.Invoices.FindAsync(invoice.InvoiceId);
        updatedInvoice!.Status.Should().Be(InvoiceStatus.Paid);
        updatedInvoice.PaymentMethod.Should().Be(PaymentMethod.Card);

        var payments = await db.Payments.Where(p => p.UserId == userId).ToListAsync();
        payments.Should().NotBeEmpty();
        payments[0].Status.Should().Be(PaymentStatus.Completed);
        payments[0].Amount.Should().Be(amount);
        payments[0].PaymentMethod.Should().Be(PaymentMethod.Card);
    }

    [Fact]
    public async Task Handle_Should_Fail_WhenPayingAnonymousInvoiceWithBalance()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<Billing.TimeCafe.Infrastructure.Data.ApplicationDbContext>();

        var visitId = Guid.NewGuid();
        var amount = 100m;

        // Создаем анонимный инвойс (UserId = null)
        var invoice = InvoiceModel.Create(null, visitId, amount).Value;
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var command = new PayInvoiceCommand(invoice.InvoiceId, PaymentMethod.Online);
        var result = await sender.Send(command);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Анонимный визит нельзя оплатить с баланса");

        var updatedInvoice = await db.Invoices.FindAsync(invoice.InvoiceId);
        updatedInvoice!.Status.Should().Be(InvoiceStatus.Pending);
    }

    [Fact]
    public async Task Handle_Should_Fail_WhenInvoiceNotFound()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new PayInvoiceCommand(Guid.NewGuid(), PaymentMethod.Online);
        var result = await sender.Send(command);

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Should().BeOfType<InvoiceNotFoundError>();
    }

    [Fact]
    public async Task Handle_Should_PayAnonymousInvoiceWithCash_AndCreateCompletedPayment()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<Billing.TimeCafe.Infrastructure.Data.ApplicationDbContext>();

        var visitId = Guid.NewGuid();
        var amount = 350m;

        // Anonymous Invoice (UserId = null)
        var invoice = InvoiceModel.Create(null, visitId, amount).Value;
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var command = new PayInvoiceCommand(invoice.InvoiceId, PaymentMethod.Cash);
        var result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updatedInvoice = await db.Invoices.FindAsync(invoice.InvoiceId);
        updatedInvoice!.Status.Should().Be(InvoiceStatus.Paid);
        updatedInvoice.PaymentMethod.Should().Be(PaymentMethod.Cash);

        // Payment must be created with UserId = null
        var payments = await db.Payments.Where(p => p.UserId == null).ToListAsync();
        payments.Should().NotBeEmpty();
        payments.Last().Status.Should().Be(PaymentStatus.Completed);
        payments.Last().Amount.Should().Be(amount);
        payments.Last().PaymentMethod.Should().Be(PaymentMethod.Cash);
    }

    [Fact]
    public async Task Handle_Should_PayAnonymousInvoiceWithCard_AndCreateCompletedPayment()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<Billing.TimeCafe.Infrastructure.Data.ApplicationDbContext>();

        var visitId = Guid.NewGuid();
        var amount = 450m;

        // Anonymous Invoice (UserId = null)
        var invoice = InvoiceModel.Create(null, visitId, amount).Value;
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        var command = new PayInvoiceCommand(invoice.InvoiceId, PaymentMethod.Card);
        var result = await sender.Send(command);

        result.IsSuccess.Should().BeTrue();

        var updatedInvoice = await db.Invoices.FindAsync(invoice.InvoiceId);
        updatedInvoice!.Status.Should().Be(InvoiceStatus.Paid);
        updatedInvoice.PaymentMethod.Should().Be(PaymentMethod.Card);

        var payments = await db.Payments.Where(p => p.UserId == null).ToListAsync();
        payments.Should().NotBeEmpty();
        payments.Last().Status.Should().Be(PaymentStatus.Completed);
        payments.Last().Amount.Should().Be(amount);
        payments.Last().PaymentMethod.Should().Be(PaymentMethod.Card);
    }
}
