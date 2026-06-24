namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;

public record GetProfileByIdQuery(Guid Id) : IQuery<ProfileDto>;

public class GetProfileByIdQueryHandler(IUnitOfWork uow) : IQueryHandler<GetProfileByIdQuery, ProfileDto>
{
    public async Task<Result<ProfileDto>> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await uow.Profiles.GetByIdAsync(request.Id, cancellationToken);
            if (profile == null)
                return Result.Fail(new ProfileNotFoundError());

            var mappedProfile = ProfilePhotoUrlMapper.WithApiUrl(profile);
            
            return Result.Ok(new ProfileDto(
                mappedProfile.UserId,
                mappedProfile.FirstName,
                mappedProfile.LastName,
                mappedProfile.MiddleName,
                mappedProfile.PhotoUrl,
                mappedProfile.BirthDate,
                (int)mappedProfile.Gender,
                (int)mappedProfile.ProfileStatus,
                mappedProfile.CreatedAt,
                mappedProfile.VisitCount,
                mappedProfile.PersonalDiscountPercent,
                mappedProfile.BanReason));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
