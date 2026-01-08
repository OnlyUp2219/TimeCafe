namespace Billing.TimeCafe.Test.Unit.CQRS.Balance.Queries;

public class GetBalanceQueryValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenUserIdEmpty()
    {
        var validator = new GetBalanceQueryValidator();
        var query = new GetBalanceQuery(InvalidData.EmptyUserId);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetBalanceQuery.UserId));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenUserIdValid()
    {
        var validator = new GetBalanceQueryValidator();
        var query = new GetBalanceQuery(Defaults.UserId);

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
