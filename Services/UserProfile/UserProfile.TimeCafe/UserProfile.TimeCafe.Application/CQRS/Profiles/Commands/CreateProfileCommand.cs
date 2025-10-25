using MediatR;
using Microsoft.Extensions.Logging;
using UserProfile.TimeCafe.Domain.Models;
using UserProfile.TimeCafe.Domain.Contracts;

namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands;

public record CreateProfileCommand(string UserId, string FirstName, string LastName, Gender Gender) : IRequest<Profile?>;

public class CreateProfileCommandHandler : IRequestHandler<CreateProfileCommand, Profile?>
{
    private readonly IUserRepositories _repository;
    private readonly ILogger<CreateProfileCommandHandler> _logger;

    public CreateProfileCommandHandler(IUserRepositories repository, ILogger<CreateProfileCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Profile?> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = new Profile
        {
            UserId = request.UserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Gender = request.Gender,
            ProfileStatus = ProfileStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Создание профиля через CQRS для UserId {UserId}", request.UserId);
        return await _repository.CreateProfileAsync(profile, cancellationToken);
    }
}