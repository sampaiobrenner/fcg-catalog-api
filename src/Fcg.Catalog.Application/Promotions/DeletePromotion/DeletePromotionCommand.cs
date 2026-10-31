using MediatR;

namespace Fcg.Catalog.Application.Promotions.DeletePromotion;

/// <summary>Desativa a promocao (soft delete).</summary>
public sealed record DeletePromotionCommand(Guid Id) : IRequest;
