namespace Billing.TimeCafe.Test.Unit.CQRS.Admin.Transactions.Queries;

public class GetTransactionsPageQueryValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageZero()
    {
        var validator = new GetTransactionsPageQueryValidator();
        var query = new GetTransactionsPageQuery(Page: 0, PageSize: 20, UserId: null);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTransactionsPageQuery.Page));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageSizeExceedsMax()
    {
        var validator = new GetTransactionsPageQueryValidator();
        var query = new GetTransactionsPageQuery(Page: 1, PageSize: 200, UserId: null);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTransactionsPageQuery.PageSize));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenValidWithoutUserId()
    {
        var validator = new GetTransactionsPageQueryValidator();
        var query = new GetTransactionsPageQuery(Page: 1, PageSize: 20, UserId: null);

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenValidWithUserId()
    {
        var validator = new GetTransactionsPageQueryValidator();
        var query = new GetTransactionsPageQuery(Page: 2, PageSize: 50, UserId: Guid.NewGuid());

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
