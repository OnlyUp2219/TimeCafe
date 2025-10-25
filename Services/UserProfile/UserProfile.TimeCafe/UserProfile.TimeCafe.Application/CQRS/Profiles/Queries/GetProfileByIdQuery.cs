using Microsoft.Extensions.Logging;

using UserProfile.TimeCafe.Domain.Contracts;
using UserProfile.TimeCafe.Domain.Models;

namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfileByIdQuery(string UserId) : IRequest<Profile?>;

public class GetProfileByIdQueryHandler(IUserRepositories repository, ILogger<GetProfileByIdQueryHandler> logger) : IRequestHandler<GetProfileByIdQuery, Profile?>
{
    private readonly IUserRepositories _repository = repository;
    private readonly ILogger<GetProfileByIdQueryHandler> _logger = logger;

    public async Task<Profile?> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Получение профиля UserId={UserId}", request.UserId);
        return await _repository.GetProfileByIdAsync(request.UserId, cancellationToken);
    }
}