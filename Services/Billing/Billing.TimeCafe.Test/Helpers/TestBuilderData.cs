namespace Billing.TimeCafe.Test.Helpers;

public class BalanceBuilder
{
    private Guid _userId = DefaultsGuid.UserId;
    private decimal _currentBalance;
    private decimal _totalDeposited;
    private decimal _totalSpent;
    private decimal _debt;

    public BalanceBuilder WithUserId(Guid id) { _userId = id; return this; }
    public BalanceBuilder WithBalance(decimal amount) { _currentBalance = amount; return this; }
    public BalanceBuilder WithDeposited(decimal amount) { _totalDeposited = amount; return this; }
    public BalanceBuilder WithSpent(decimal amount) { _totalSpent = amount; return this; }
    public BalanceBuilder WithDebt(decimal amount) { _debt = amount; return this; }

    public Balance Build() => new()
    {
        UserId = _userId,
        CurrentBalance = _currentBalance,
        TotalDeposited = _totalDeposited,
        TotalSpent = _totalSpent,
        Debt = _debt,
        CreatedAt = DateTimeOffset.UtcNow,
        LastUpdated = DateTimeOffset.UtcNow
    };

    public CreateBalanceCommand BuildCreateCommand() => new(_userId);
}

public class TransactionBuilder
{
    private Guid _transactionId = Guid.NewGuid();
    private Guid _userId = DefaultsGuid.UserId;
    private decimal _amount = DefaultsGuid.DefaultAmount;
    private TransactionType _type = TransactionType.Deposit;
    private TransactionSource _source = TransactionSource.Manual;
    private Guid? _sourceId;
    private string? _comment;

    public TransactionBuilder WithId(Guid id) { _transactionId = id; return this; }
    public TransactionBuilder WithUserId(Guid id) { _userId = id; return this; }
    public TransactionBuilder WithAmount(decimal amount) { _amount = amount; return this; }
    public TransactionBuilder WithType(TransactionType type) { _type = type; return this; }
    public TransactionBuilder WithSource(TransactionSource source) { _source = source; return this; }
    public TransactionBuilder WithSourceId(Guid? id) { _sourceId = id; return this; }
    public TransactionBuilder WithComment(string? comment) { _comment = comment; return this; }
    public TransactionBuilder AsWithdrawal() { _type = TransactionType.Withdrawal; return this; }

    public Transaction Build() => new()
    {
        TransactionId = _transactionId,
        UserId = _userId,
        Amount = _amount,
        Type = _type,
        Source = _source,
        SourceId = _sourceId,
        Comment = _comment,
        Status = TransactionStatus.Completed,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public AdjustBalanceCommand BuildCommand() => new(_userId, _amount, _type, _source, _sourceId, _comment);
}

public class PaymentBuilder
{
    private Guid _paymentId = DefaultsGuid.PaymentId;
    private Guid _userId = DefaultsGuid.UserId;
    private decimal _amount = DefaultsGuid.DefaultAmount;
    private string? _returnUrl;
    private string? _description;

    public PaymentBuilder WithId(Guid id) { _paymentId = id; return this; }
    public PaymentBuilder WithUserId(Guid id) { _userId = id; return this; }
    public PaymentBuilder WithAmount(decimal amount) { _amount = amount; return this; }
    public PaymentBuilder WithReturnUrl(string? url) { _returnUrl = url; return this; }
    public PaymentBuilder WithDescription(string? desc) { _description = desc; return this; }

    public Payment Build() => new()
    {
        PaymentId = _paymentId,
        UserId = _userId,
        Amount = _amount,
        PaymentMethod = PaymentMethod.Online,
        Status = PaymentStatus.Pending,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public InitializeStripePaymentCommand BuildStripePaymentCommand() => new(_userId, _amount, _returnUrl, _description);
    public InitializeStripeCheckoutCommand BuildStripeCheckoutCommand(string? successUrl = null, string? cancelUrl = null) => new(_userId, _amount, successUrl, cancelUrl, _description);
}
