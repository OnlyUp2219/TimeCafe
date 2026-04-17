namespace Billing.TimeCafe.Test.Unit.CQRS.Admin.Payments.Queries;

public class GetPaymentsPageQueryValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageZero()
    {
        var validator = new GetPaymentsPageQueryValidator();
        var query = new GetPaymentsPageQuery(Page: 0, PageSize: 20, UserId: null);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetPaymentsPageQuery.Page));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageSizeNegative()
    {
        var validator = new GetPaymentsPageQueryValidator();
        var query = new GetPaymentsPageQuery(Page: 1, PageSize: -5, UserId: null);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetPaymentsPageQuery.PageSize));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageSizeExceedsMax()
    {
        var validator = new GetPaymentsPageQueryValidator();
        var query = new GetPaymentsPageQuery(Page: 1, PageSize: 101, UserId: null);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetPaymentsPageQuery.PageSize));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenValidWithoutUserId()
    {
        var validator = new GetPaymentsPageQueryValidator();
        var query = new GetPaymentsPageQuery(Page: 1, PageSize: 20, UserId: null);

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenValidWithUserId()
    {
        var validator = new GetPaymentsPageQueryValidator();
        var query = new GetPaymentsPageQuery(Page: 3, PageSize: 100, UserId: Guid.NewGuid());

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
