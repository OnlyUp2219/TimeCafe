namespace UserProfile.TimeCafe.Application.Contracts;

public interface IUnitOfWork : BuildingBlocks.Contracts.IUnitOfWork
{
    IUserRepositories Profiles { get; }
    IAdditionalInfoRepository AdditionalInfos { get; }
}
