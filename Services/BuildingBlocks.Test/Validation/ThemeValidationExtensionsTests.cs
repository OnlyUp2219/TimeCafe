namespace BuildingBlocks.Test.Validation;

public class ThemeValidationExtensionsTests
{
    private class TestModel
    {
        public string? Emoji { get; set; }
    }

    [Fact]
    public void ValidEmoji_Should_Fail_WhenTooLong()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Emoji).ValidEmoji(2);

        var model = new TestModel { Emoji = "😊😊😊" };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Эмодзи не может превышать 2 символов");
    }

    [Fact]
    public void ValidEmoji_Should_Succeed_WhenValid()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Emoji).ValidEmoji(2);

        var model = new TestModel { Emoji = "😊" };
        var result = validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidEmoji_Should_Succeed_WhenNull()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Emoji).ValidEmoji();

        var model = new TestModel { Emoji = null };
        var result = validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }
}
