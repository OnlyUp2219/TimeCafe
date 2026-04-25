namespace Auth.TimeCafe.Test.Unit.Admin.Queries;

public class GetUserByIdQueryValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenUserIdEmpty()
    {
        var validator = new GetUserByIdQueryValidator();
        var query = new GetUserByIdQuery(Guid.Empty);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUserByIdQuery.UserId));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenUserIdValid()
    {
        var validator = new GetUserByIdQueryValidator();
        var query = new GetUserByIdQuery(Guid.NewGuid());

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
