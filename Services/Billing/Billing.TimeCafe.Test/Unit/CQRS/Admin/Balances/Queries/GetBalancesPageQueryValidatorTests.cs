namespace Billing.TimeCafe.Test.Unit.CQRS.Admin.Balances.Queries;

public class GetBalancesPageQueryValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageZero()
    {
        var validator = new GetBalancesPageQueryValidator();
        var query = new GetBalancesPageQuery(Page: 0, PageSize: 20);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetBalancesPageQuery.Page));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageNegative()
    {
        var validator = new GetBalancesPageQueryValidator();
        var query = new GetBalancesPageQuery(Page: -1, PageSize: 20);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetBalancesPageQuery.Page));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageSizeZero()
    {
        var validator = new GetBalancesPageQueryValidator();
        var query = new GetBalancesPageQuery(Page: 1, PageSize: 0);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetBalancesPageQuery.PageSize));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageSizeExceedsMax()
    {
        var validator = new GetBalancesPageQueryValidator();
        var query = new GetBalancesPageQuery(Page: 1, PageSize: 101);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetBalancesPageQuery.PageSize));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenValid()
    {
        var validator = new GetBalancesPageQueryValidator();
        var query = new GetBalancesPageQuery(Page: 1, PageSize: 20);

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
