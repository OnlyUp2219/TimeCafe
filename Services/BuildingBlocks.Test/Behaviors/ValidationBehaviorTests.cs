namespace BuildingBlocks.Test.Behaviors;

using FluentValidation.Results;

public class ValidationBehaviorTests
{
    public class TestRequest : IRequest<string> { }

    [Fact]
    public async Task Handle_Should_CallNext_WhenNoValidators()
    {
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var logger = new Mock<ILogger<ValidationBehavior<TestRequest, string>>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators, logger.Object);

        var request = new TestRequest();
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("success");
        };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be("success");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_ThrowValidationException_WhenValidationFails()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        var failures = new List<ValidationFailure> { new ValidationFailure("Prop", "Error") };
        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var validators = new[] { validator.Object };
        var logger = new Mock<ILogger<ValidationBehavior<TestRequest, string>>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators, logger.Object);

        var request = new TestRequest();
        RequestHandlerDelegate<string> next = () => Task.FromResult("success");

        var act = () => behavior.Handle(request, next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_Should_CallNext_WhenValidationSucceeds()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { validator.Object };
        var logger = new Mock<ILogger<ValidationBehavior<TestRequest, string>>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators, logger.Object);

        var request = new TestRequest();
        RequestHandlerDelegate<string> next = () => Task.FromResult("success");

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Should().Be("success");
    }
}
