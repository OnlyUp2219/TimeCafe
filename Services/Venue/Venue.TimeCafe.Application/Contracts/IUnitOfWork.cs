namespace Venue.TimeCafe.Application.Contracts;

public interface IUnitOfWork : BuildingBlocks.Contracts.IUnitOfWork
{
    ITariffRepository Tariffs { get; }
    IPromotionRepository Promotions { get; }
    IThemeRepository Themes { get; }
    IVisitRepository Visits { get; }
    IUserLoyaltyRepository UserLoyalties { get; }
    IResourceRepository Resources { get; }
    IResourceGroupRepository ResourceGroups { get; }
}
