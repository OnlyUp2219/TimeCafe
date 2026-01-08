namespace Billing.TimeCafe.Test.Unit.CQRS.Balance.Commands;

public class AdjustBalanceCommandValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenUserIdEmpty()
    {
        var validator = new AdjustBalanceCommandValidator();
        var cmd = new AdjustBalanceCommand(InvalidData.EmptyUserId, Defaults.SmallAmount, TransactionType.Deposit, TransactionSource.Manual);

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdjustBalanceCommand.UserId));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenAmountNotPositive()
    {
        var validator = new AdjustBalanceCommandValidator();
        var cmdZero = new AdjustBalanceCommand(Defaults.UserId, 0m, TransactionType.Deposit, TransactionSource.Manual);
        var cmdNeg = new AdjustBalanceCommand(Defaults.UserId, -1m, TransactionType.Deposit, TransactionSource.Manual);

        validator.Validate(cmdZero).IsValid.Should().BeFalse();
        validator.Validate(cmdNeg).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenCommentTooLong()
    {
        var validator = new AdjustBalanceCommandValidator();
        var longComment = new string('x', 501);
        var cmd = new AdjustBalanceCommand(Defaults.UserId, Defaults.SmallAmount, TransactionType.Deposit, TransactionSource.Manual, Comment: longComment);

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdjustBalanceCommand.Comment));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenValid()
    {
        var validator = new AdjustBalanceCommandValidator();
        var cmd = new AdjustBalanceCommand(Defaults.UserId, Defaults.SmallAmount, TransactionType.Deposit, TransactionSource.Manual, Comment: "ok");

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
