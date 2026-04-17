namespace Auth.TimeCafe.Test.Unit.Admin.Commands;

public class DeleteUserCommandValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenUserIdEmpty()
    {
        var validator = new DeleteUserCommandValidator();
        var command = new DeleteUserCommand(Guid.Empty);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteUserCommand.UserId));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenUserIdValid()
    {
        var validator = new DeleteUserCommandValidator();
        var command = new DeleteUserCommand(Guid.NewGuid());

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
