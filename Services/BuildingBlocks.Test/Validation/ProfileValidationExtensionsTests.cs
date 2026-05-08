namespace BuildingBlocks.Test.Validation;

public class ProfileValidationExtensionsTests
{
    private class TestModel
    {
        public DateOnly? BirthDate { get; set; }
    }

    [Fact]
    public void ValidBirthDate_Should_Fail_WhenInFuture()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.BirthDate).ValidBirthDate();

        var model = new TestModel { BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Дата рождения не может быть в будущем");
    }

    [Fact]
    public void ValidBirthDate_Should_Succeed_WhenInPast()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.BirthDate).ValidBirthDate();

        var model = new TestModel { BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) };
        var result = validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidBirthDate_Should_Succeed_WhenNull()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.BirthDate).ValidBirthDate();

        var model = new TestModel { BirthDate = null };
        var result = validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }
}
