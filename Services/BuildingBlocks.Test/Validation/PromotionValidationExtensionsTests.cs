namespace BuildingBlocks.Test.Validation;

public class PromotionValidationExtensionsTests
{
    private class TestModel
    {
        public decimal? DiscountPercent { get; set; }
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset ValidTo { get; set; }
    }

    [Fact]
    public void ValidDiscountPercent_Should_Fail_WhenZero()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.DiscountPercent).ValidDiscountPercent();

        var model = new TestModel { DiscountPercent = 0 };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Процент скидки должен быть больше 0");
    }

    [Fact]
    public void ValidDiscountPercent_Should_Fail_WhenGreaterThan100()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.DiscountPercent).ValidDiscountPercent();

        var model = new TestModel { DiscountPercent = 101 };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Процент скидки не может превышать 100");
    }

    [Fact]
    public void ValidFromBeforeValidTo_Should_Fail_WhenFromIsAfterTo()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.ValidFrom).ValidFromBeforeValidTo(x => x.ValidTo);

        var now = DateTimeOffset.UtcNow;
        var model = new TestModel { ValidFrom = now.AddDays(1), ValidTo = now };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Дата начала должна быть раньше даты окончания");
    }

    [Fact]
    public void ValidFromBeforeValidTo_Should_Succeed_WhenFromIsBeforeTo()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.ValidFrom).ValidFromBeforeValidTo(x => x.ValidTo);

        var now = DateTimeOffset.UtcNow;
        var model = new TestModel { ValidFrom = now, ValidTo = now.AddDays(1) };
        var result = validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }
}
