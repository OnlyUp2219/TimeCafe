namespace Billing.TimeCafe.Test.Unit.CQRS.Transactions.Queries;

public class GetTransactionHistoryQueryValidatorTests
{
    [Fact]
    public void Validator_Validate_Should_Fail_WhenUserIdEmpty()
    {
        var validator = new GetTransactionHistoryQueryValidator();
        var query = new GetTransactionHistoryQuery(InvalidData.EmptyUserId, Page: 1, PageSize: 10);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTransactionHistoryQuery.UserId));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageZero()
    {
        var validator = new GetTransactionHistoryQueryValidator();
        var query = new GetTransactionHistoryQuery(Defaults.UserId, Page: 0, PageSize: 10);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTransactionHistoryQuery.Page));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageNegative()
    {
        var validator = new GetTransactionHistoryQueryValidator();
        var query = new GetTransactionHistoryQuery(Defaults.UserId, Page: -1, PageSize: 10);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTransactionHistoryQuery.Page));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageSizeZero()
    {
        var validator = new GetTransactionHistoryQueryValidator();
        var query = new GetTransactionHistoryQuery(Defaults.UserId, Page: 1, PageSize: 0);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTransactionHistoryQuery.PageSize));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageSizeNegative()
    {
        var validator = new GetTransactionHistoryQueryValidator();
        var query = new GetTransactionHistoryQuery(Defaults.UserId, Page: 1, PageSize: -10);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTransactionHistoryQuery.PageSize));
    }

    [Fact]
    public void Validator_Validate_Should_Fail_WhenPageSizeGreaterThan100()
    {
        var validator = new GetTransactionHistoryQueryValidator();
        var query = new GetTransactionHistoryQuery(Defaults.UserId, Page: 1, PageSize: 101);

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetTransactionHistoryQuery.PageSize));
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenAllParametersValid()
    {
        var validator = new GetTransactionHistoryQueryValidator();
        var query = new GetTransactionHistoryQuery(Defaults.UserId, Page: 1, PageSize: 10);

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validator_Validate_Should_Pass_WhenPageSizeIs100()
    {
        var validator = new GetTransactionHistoryQueryValidator();
        var query = new GetTransactionHistoryQuery(Defaults.UserId, Page: 1, PageSize: 100);

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
