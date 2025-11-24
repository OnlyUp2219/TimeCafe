namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Commands
{
    public record CreateProfileCommand(string UserId, string FirstName, string LastName, Gender Gender) : IRequest<Profile?>;

    public class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
    {
        public CreateProfileCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().MaximumLength(64);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(128);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(128);
        }
    }
    public class CreateProfileCommandHandler(IUserRepositories repository) : IRequestHandler<CreateProfileCommand, Profile?>
    {
        private readonly IUserRepositories _repository = repository;

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

            return await _repository.CreateProfileAsync(profile, cancellationToken);
        }
    }
}