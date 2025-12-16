namespace Venue.TimeCafe.Test.Unit.CQRS.VisitsCqrs.Queries;

using Venue.TimeCafe.Domain.DTOs;
using Venue.TimeCafe.Test.Integration.Helpers;

public class GetVisitByIdQueryTests : BaseCqrsHandlerTest
{
    private readonly GetVisitByIdQueryHandler _handler;

    public GetVisitByIdQueryTests()
    {
        _handler = new GetVisitByIdQueryHandler(VisitRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_Should_ReturnSuccess_WhenVisitFound()
    {
        var visitId = Guid.NewGuid();
        var query = new GetVisitByIdQuery(visitId.ToString());
        var visitDto = new VisitWithTariffDto
        {
            VisitId = visitId,
            UserId = TestData.ExistingVisits.Visit1UserId,
            TariffId = Guid.NewGuid(),
            EntryTime = DateTimeOffset.UtcNow,
            Status = VisitStatus.Active
        };

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync(visitDto);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Visit.Should().NotBeNull();
        result.Visit!.UserId.Should().Be(TestData.ExistingVisits.Visit1UserId);
    }

    [Fact]
    public async Task Handler_Should_ReturnNotFound_WhenVisitDoesNotExist()
    {
        var visitId = TestData.NonExistingIds.NonExistingVisitId;
        var query = new GetVisitByIdQuery(visitId.ToString());

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ReturnsAsync((VisitWithTariffDto?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("VisitNotFound");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handler_Should_ReturnFailed_WhenExceptionThrown()
    {
        var visitId = Guid.NewGuid();
        var query = new GetVisitByIdQuery(visitId.ToString());

        VisitRepositoryMock.Setup(r => r.GetByIdAsync(visitId)).ThrowsAsync(new Exception());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("GetVisitFailed");
        result.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("not-a-guid", false)]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", true)]
    public async Task Validator_Should_ValidateCorrectly(string visitId, bool isValid)
    {
        var query = new GetVisitByIdQuery(visitId);
        var validator = new GetVisitByIdQueryValidator();

        var result = await validator.ValidateAsync(query);

        result.IsValid.Should().Be(isValid);
    }
}
