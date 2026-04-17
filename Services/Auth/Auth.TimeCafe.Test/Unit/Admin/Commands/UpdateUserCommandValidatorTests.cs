namespace Auth.TimeCafe.Test.Unit.Admin.Commands;

public class UpdateUserCommandValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenUserIdEmpty()
    {
        var validator = new UpdateUserCommandValidator();
        var command = new UpdateUserCommand(Guid.Empty, null, null, null, null, null);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.UserId));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenEmailInvalid()
    {
        var validator = new UpdateUserCommandValidator();
        var command = new UpdateUserCommand(Guid.NewGuid(), "not-an-email", null, null, null, null);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.Email));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenValidWithEmail()
    {
        var validator = new UpdateUserCommandValidator();
        var command = new UpdateUserCommand(Guid.NewGuid(), "test@example.com", null, null, null, null);

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenValidWithAllNulls()
    {
        var validator = new UpdateUserCommandValidator();
        var command = new UpdateUserCommand(Guid.NewGuid(), null, null, null, null, null);

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenValidWithLockout()
    {
        var validator = new UpdateUserCommandValidator();
        var command = new UpdateUserCommand(
            Guid.NewGuid(),
            null,
            "newusername",
            true,
            true,
            DateTimeOffset.UtcNow.AddDays(7));

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
