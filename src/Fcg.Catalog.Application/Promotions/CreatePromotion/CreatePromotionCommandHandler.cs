using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application._Shared.Parsers;
using Fcg.Catalog.Application.Properties;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Promotions;
using MediatR;

namespace Fcg.Catalog.Application.Promotions.CreatePromotion;

internal sealed class CreatePromotionCommandHandler : IRequestHandler<CreatePromotionCommand, PromotionResponse>
{
    private readonly IPromotionRepository _promotions;
    private readonly IGameRepository _games;
    private readonly ICatalogDbContext _context;
    private readonly IParser<Promotion, PromotionResponse> _parser;

    public CreatePromotionCommandHandler(
        IPromotionRepository promotions,
        IGameRepository games,
        ICatalogDbContext context,
        IParser<Promotion, PromotionResponse> parser)
    {
        _promotions = promotions;
        _games = games;
        _context = context;
        _parser = parser;
    }

    public async Task<PromotionResponse> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var gameIds = request.GameIds.Distinct().ToList();
        var existing = await _games.GetExistingIdsAsync(gameIds, cancellationToken);

        var missing = gameIds.Except(existing).ToList();
        if (missing.Count > 0)
            throw new NotFoundException(ApplicationResources.Jogo, string.Join(", ", missing));

        var promotion = Promotion.Create(request.Name, request.DiscountPercent, request.StartsAt, request.EndsAt, gameIds);

        await _promotions.AddAsync(promotion, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _parser.Parse(promotion)!;
    }
}
