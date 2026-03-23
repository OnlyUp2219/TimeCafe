namespace Billing.TimeCafe.Test.Unit.CQRS.Balance.Queries;

public class GetUserDebtQueryValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenUserIdEmpty()
    {
        var validator = new GetUserDebtQueryValidator();
        var query = new GetUserDebtQuery(InvalidDataGuid.EmptyUserId);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUserDebtQuery.UserId));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenUserIdDefault()
    {
        var validator = new GetUserDebtQueryValidator();
        var query = new GetUserDebtQuery(InvalidDataGuid.EmptyUserId);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUserDebtQuery.UserId));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenUserIdValid()
    {
        var validator = new GetUserDebtQueryValidator();
        var query = new GetUserDebtQuery(DefaultsGuid.UserId);

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
