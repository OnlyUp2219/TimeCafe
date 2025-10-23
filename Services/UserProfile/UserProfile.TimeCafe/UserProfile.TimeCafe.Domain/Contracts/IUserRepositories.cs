using UserProfile.TimeCafe.Domain.Models;

namespace UserProfile.TimeCafe.Domain.Contracts;

public interface IUserRepositories
{
    Task<IEnumerable<Profile>> GetAllProfiles();
    Task<IEnumerable<Profile>> GetProfilesPage(int pageNumber, int pageSize);
    Task<int> GetTotalPage();
    Task<Profile> GetClientById(int clientId);
    Task<Profile> CreateProfile(Profile client);
    Task<Profile> UpdateProfile(Profile client);
    Task<bool> DeleteProfilet(int clientId);

}
