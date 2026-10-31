using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Properties;
using Fcg.Catalog.Domain.Games;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Catalog.Application.Games.GetGameById;

internal sealed class GetGameByIdQueryHandler : IRequestHandler<GetGameByIdQuery, GameResponse>
{
    private readonly ICatalogDbContext _context;

    public GetGameByIdQueryHandler(ICatalogDbContext context) => _context = context;

    public async Task<GameResponse> Handle(GetGameByIdQuery request, CancellationToken cancellationToken)
        => await _context.DataSet<GamePersistenceModel>()
               .Where(game => game.Id == request.Id)
               .Select(game => new GameResponse(
                   game.Id,
                   game.Title,
                   game.Description,
                   game.Genre,
                   game.Price,
                   game.ReleaseDate,
                   game.Active,
                   game.CreatedAt))
               .FirstOrDefaultAsync(cancellationToken)
           ?? throw new NotFoundException(ApplicationResources.Jogo, request.Id);
}
