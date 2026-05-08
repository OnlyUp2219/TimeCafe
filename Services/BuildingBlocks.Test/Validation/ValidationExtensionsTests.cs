namespace BuildingBlocks.Test.Validation;

public class ValidationExtensionsTests
{
    private class TestModel
    {
        public string Id { get; set; } = "";
        public Guid GuidId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string Email { get; set; } = "";
    }

    [Fact]
    public void ValidEntityId_Should_Fail_WhenEmpty()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Id).ValidEntityId("Invalid ID");

        var model = new TestModel { Id = "" };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Invalid ID");
    }

    [Fact]
    public void ValidEntityId_Should_Fail_WhenNotGuid()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Id).ValidEntityId("Invalid ID");

        var model = new TestModel { Id = "not-a-guid" };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Invalid ID");
    }

    [Fact]
    public void ValidEntityId_Should_Succeed_WhenValidGuid()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Id).ValidEntityId("Invalid ID");

        var model = new TestModel { Id = Guid.NewGuid().ToString() };
        var result = validator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidGuidEntityId_Should_Fail_WhenEmpty()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.GuidId).ValidGuidEntityId("Invalid GUID");

        var model = new TestModel { GuidId = Guid.Empty };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Invalid GUID");
    }

    [Fact]
    public void ValidPageNumber_Should_Fail_WhenZeroOrLess()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.PageNumber).ValidPageNumber();

        var model = new TestModel { PageNumber = 0 };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Страница должна быть больше 0");
    }

    [Fact]
    public void ValidEmail_Should_Fail_WhenInvalid()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.Email).ValidEmail();

        var model = new TestModel { Email = "not-an-email" };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Некорректный формат email");
    }
}
