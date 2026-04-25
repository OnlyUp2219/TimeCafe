namespace UserProfile.TimeCafe.API.Endpoints.AdditionalInfos;

public class DeleteAdditionalInfo : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/infos/{infoId:guid}", async (
            [FromServices] ISender sender,
            Guid infoId) =>
        {
            var command = new DeleteAdditionalInfoCommand(infoId);
            var result = await sender.Send(command);
            return result.ToHttpResult(() => TypedResults.NoContent());
        })
        .WithTags("AdditionalInfos")
        .WithName("DeleteAdditionalInfo")
        .WithSummary("Удалить запись доп. информации")
        .WithDescription("Удаляет запись дополнительной информации по идентификатору.")
        .Produces(204)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.UserProfileAdditionalInfoDelete));
    }
}

