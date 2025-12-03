namespace Venue.TimeCafe.Test.Helpers;

public abstract class BaseCqrsHandlerTest
{
    protected readonly Mock<ITariffRepository> TariffRepositoryMock;
    protected readonly Mock<IPromotionRepository> PromotionRepositoryMock;
    protected readonly Mock<IThemeRepository> ThemeRepositoryMock;
    protected readonly Mock<IVisitRepository> VisitRepositoryMock;

    protected BaseCqrsHandlerTest()
    {
        TariffRepositoryMock = new Mock<ITariffRepository>();
        PromotionRepositoryMock = new Mock<IPromotionRepository>();
        ThemeRepositoryMock = new Mock<IThemeRepository>();
        VisitRepositoryMock = new Mock<IVisitRepository>();
    }
}
