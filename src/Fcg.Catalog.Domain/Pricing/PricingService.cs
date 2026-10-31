using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Promotions;

namespace Fcg.Catalog.Domain.Pricing;

/// <summary>Servico de dominio que calcula o preco efetivo de um jogo (maior desconto ativo no momento).</summary>
public sealed class PricingService
{
    /// <summary>Escolhe, entre as candidatas, a promocao ativa no momento com o maior desconto para o jogo.</summary>
    public Promotion? FindBestPromotion(Guid gameId, IEnumerable<Promotion> candidates, DateTime moment)
        => candidates
            .Where(promotion => promotion.AppliesTo(gameId) && promotion.IsActiveOn(moment))
            .OrderByDescending(promotion => promotion.DiscountPercent)
            .FirstOrDefault();

    /// <summary>Preco do jogo com o desconto da promocao ativa aplicado (ou o preco cheio, sem promocao).</summary>
    public Money GetEffectivePrice(Game game, Promotion? activePromotion)
    {
        ArgumentNullException.ThrowIfNull(game);

        if (activePromotion is null || !activePromotion.AppliesTo(game.Id()))
            return game.Price;

        return game.Price.ApplyDiscount(activePromotion.DiscountPercent);
    }

    public Money GetEffectivePrice(Game game, IEnumerable<Promotion> candidates, DateTime moment)
        => GetEffectivePrice(game, FindBestPromotion(game.Id(), candidates, moment));
}
