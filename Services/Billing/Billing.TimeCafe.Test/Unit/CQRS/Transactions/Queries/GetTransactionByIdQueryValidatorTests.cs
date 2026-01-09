namespace Billing.TimeCafe.Test.Unit.CQRS.Transactions.Queries;

public class GetTransactionByIdQueryValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenTransactionIdEmpty()
    {
        var validator = new GetTransactionByIdQueryValidator();
        var query = new GetTransactionByIdQuery(InvalidData.EmptyUserId);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTransactionByIdQuery.TransactionId));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenTransactionIdValid()
    {
        var validator = new GetTransactionByIdQueryValidator();
        var query = new GetTransactionByIdQuery(Defaults.TransactionId);

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
