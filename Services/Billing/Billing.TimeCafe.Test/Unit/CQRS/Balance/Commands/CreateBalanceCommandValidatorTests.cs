namespace Billing.TimeCafe.Test.Unit.CQRS.Balance.Commands;

public class CreateBalanceCommandValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenUserIdEmpty()
    {
        var validator = new CreateBalanceCommandValidator();
        var cmd = new CreateBalanceCommand(InvalidData.EmptyUserId.ToString());

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateBalanceCommand.UserId));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenUserIdValid()
    {
        var validator = new CreateBalanceCommandValidator();
        var cmd = new CreateBalanceCommand(Defaults.UserId);

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
