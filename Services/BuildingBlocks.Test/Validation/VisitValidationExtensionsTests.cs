namespace BuildingBlocks.Test.Validation;

public class VisitValidationExtensionsTests
{
    private class TestModel
    {
        public int? PlannedMinutes { get; set; }
        public DateTimeOffset EntryTime { get; set; }
        public DateTimeOffset? ExitTime { get; set; }
        public decimal? CalculatedCost { get; set; }
    }

    [Fact]
    public void ValidPlannedMinutes_Should_Fail_WhenZero()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.PlannedMinutes).ValidPlannedMinutes();

        var model = new TestModel { PlannedMinutes = 0 };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Время посещения должно быть больше 0 минут");
    }

    [Fact]
    public void ValidExitTime_Should_Fail_WhenBeforeEntry()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.ExitTime).ValidExitTime(x => x.EntryTime);

        var now = DateTimeOffset.UtcNow;
        var model = new TestModel { EntryTime = now, ExitTime = now.AddMinutes(-1) };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Время выхода не может быть раньше времени входа");
    }

    [Fact]
    public void ValidEntryTime_Should_Fail_WhenDefault()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.EntryTime).ValidEntryTime();

        var model = new TestModel { EntryTime = default };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Время входа некорректно");
    }

    [Fact]
    public void ValidCalculatedCost_Should_Fail_WhenNegative()
    {
        var validator = new InlineValidator<TestModel>();
        validator.RuleFor(x => x.CalculatedCost).ValidCalculatedCost();

        var model = new TestModel { CalculatedCost = -1 };
        var result = validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Стоимость не может быть отрицательной");
    }
}
