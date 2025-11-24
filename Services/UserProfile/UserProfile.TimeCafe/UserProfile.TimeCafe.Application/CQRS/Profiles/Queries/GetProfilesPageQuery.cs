using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record class GetProfilesPageQuery(int PageNumber, int PageSize) : IRequest<IEnumerable<Profile?>>;

public class GetProfilesPageQueryValidator : AbstractValidator<GetProfilesPageQuery>
{
    public GetProfilesPageQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.");
    }
}

public class GetProfilesPageQueryHandler(IUserRepositories repositories) : IRequestHandler<GetProfilesPageQuery, IEnumerable<Profile?>>
{
    private readonly IUserRepositories _repositories = repositories;
    public async Task<IEnumerable<Profile?>> Handle(GetProfilesPageQuery request, CancellationToken cancellationToken)
    {
        return await _repositories.GetProfilesPageAsync(request.PageNumber, request.PageSize, cancellationToken).ConfigureAwait(false);
    }
}