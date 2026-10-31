using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application._Shared.Parsers;
using Fcg.Catalog.Application.Properties;
using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Games;
using MediatR;

namespace Fcg.Catalog.Application.Games.UpdateGame;

internal sealed class UpdateGameCommandHandler : IRequestHandler<UpdateGameCommand, GameResponse>
{
    private readonly IGameRepository _games;
    private readonly ICatalogDbContext _context;
    private readonly IParser<Game, GameResponse> _parser;

    public UpdateGameCommandHandler(IGameRepository games, ICatalogDbContext context, IParser<Game, GameResponse> parser)
    {
        _games = games;
        _context = context;
        _parser = parser;
    }

    public async Task<GameResponse> Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _games.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new NotFoundException(ApplicationResources.Jogo, request.Id);

        game.Update(request.Title, request.Description, request.Genre, Money.Create(request.Price), request.ReleaseDate);
        await _context.SaveChangesAsync(cancellationToken);

        return _parser.Parse(game)!;
    }
}
