using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Properties;
using Fcg.Catalog.Domain.Games;
using MediatR;

namespace Fcg.Catalog.Application.Games.DeleteGame;

internal sealed class DeleteGameCommandHandler : IRequestHandler<DeleteGameCommand>
{
    private readonly IGameRepository _games;
    private readonly ICatalogDbContext _context;

    public DeleteGameCommandHandler(IGameRepository games, ICatalogDbContext context)
    {
        _games = games;
        _context = context;
    }

    public async Task Handle(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _games.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new NotFoundException(ApplicationResources.Jogo, request.Id);

        game.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
