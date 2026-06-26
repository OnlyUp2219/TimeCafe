namespace BuildingBlocks.Test.Validation;

public class PaymentValidationExtensionsTests
{
    private class TestModel
    {
        public decimal Amount { get; set; }
        public string? Url { get; set; }
    }

    [Fact]
    public void ValidPaymentAmount_Should_Fail_WhenZero()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Amount).ValidPaymentAmount();

        var model = new TestModel { Amount = 0 };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Сумма должна быть больше нуля");
    }

    [Fact]
    public void ValidPaymentAmount_Should_Fail_WhenLessThanMin()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Amount).ValidPaymentAmount(100);

        var model = new TestModel { Amount = 50 };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Минимальная сумма платежа 100 Br");
    }

    [Fact]
    public void ValidUrlWithPlaceholder_Should_Succeed_WithPlaceholder()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Url).ValidUrlWithPlaceholder();

        var model = new TestModel { Url = "https://example.com/success?session={CHECKOUT_SESSION_ID}" };
        var result = validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidUrlWithPlaceholder_Should_Fail_WhenInvalidUrl()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Url).ValidUrlWithPlaceholder();

        var model = new TestModel { Url = "not-a-url" };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "URL некорректен");
    }

    [Fact]
    public void ValidUrlWithPlaceholder_Should_Succeed_WhenNull()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Url).ValidUrlWithPlaceholder();

        var model = new TestModel { Url = null };
        var result = validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }
}
