namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record class GetTotalPageQuery() : IRequest<int>;

public class GetTotalPageQueryValidator : AbstractValidator<GetTotalPageQuery>
{
    public GetTotalPageQueryValidator()
    {

    }
}

public class GetTotalPageQueryHandler(IUserRepositories repositories) : IRequestHandler<GetTotalPageQuery, int>
{
    private readonly IUserRepositories _repositories = repositories;    
    public async Task<int> Handle(GetTotalPageQuery request, CancellationToken cancellationToken)
    {
       return await _repositories.GetTotalPageAsync(cancellationToken).ConfigureAwait(false);
    }
}