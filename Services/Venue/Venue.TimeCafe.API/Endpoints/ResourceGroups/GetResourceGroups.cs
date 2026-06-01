using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Venue.TimeCafe.Application.CQRS.ResourceGroups.Queries;
using BuildingBlocks.Extensions;

namespace Venue.TimeCafe.API.Endpoints.ResourceGroups;

public class GetResourceGroups : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/resource-groups", async ([FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetResourceGroupsQuery());
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("ResourceGroups")
        .WithName("GetResourceGroups")
        .WithSummary("Получить все зоны")
        .WithDescription("Получает список всех пространственных зон заведения.")
        .Produces(200);
    }
}
