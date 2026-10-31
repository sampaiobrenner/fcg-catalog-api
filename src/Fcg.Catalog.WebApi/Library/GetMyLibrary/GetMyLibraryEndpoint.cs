using Fcg.Catalog.Application.Library.GetMyLibrary;
using Fcg.Catalog.WebApi._Shared.Endpoints;
using MediatR;

namespace Fcg.Catalog.WebApi.Library.GetMyLibrary;

internal sealed class GetMyLibraryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/library", async (ISender sender, CancellationToken cancellationToken)
                => Results.Ok(await sender.Send(new GetMyLibraryQuery(), cancellationToken)))
            .RequireAuthorization()
            .WithTags(EndpointTags.Library)
            .WithName("GetMyLibrary")
            .WithSummary("Lista os jogos da biblioteca do usuario autenticado.")
            .Produces<IReadOnlyCollection<LibraryItemResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
