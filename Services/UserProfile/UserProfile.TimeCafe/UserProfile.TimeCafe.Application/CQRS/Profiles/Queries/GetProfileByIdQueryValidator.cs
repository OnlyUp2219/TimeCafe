namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public class GetProfileByIdQueryValidator : AbstractValidator<GetProfileByIdQuery>
{
    public GetProfileByIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(64);
    }
}