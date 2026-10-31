using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application._Shared.Parsers;
using Fcg.Catalog.Application.Properties;
using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Games;
using MediatR;

namespace Fcg.Catalog.Application.Games.CreateGame;

internal sealed class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, GameResponse>
{
    private readonly IGameRepository _games;
    private readonly ICatalogDbContext _context;
    private readonly IParser<Game, GameResponse> _parser;

    public CreateGameCommandHandler(IGameRepository games, ICatalogDbContext context, IParser<Game, GameResponse> parser)
    {
        _games = games;
        _context = context;
        _parser = parser;
    }

    public async Task<GameResponse> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();

        if (await _games.TitleExistsAsync(title, cancellationToken))
            throw new ConflictException(string.Format(ApplicationResources.JogoComTituloJaCadastrado, title));

        var game = Game.Create(title, request.Description, request.Genre, Money.Create(request.Price), request.ReleaseDate);

        await _games.AddAsync(game, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _parser.Parse(game)!;
    }
}
