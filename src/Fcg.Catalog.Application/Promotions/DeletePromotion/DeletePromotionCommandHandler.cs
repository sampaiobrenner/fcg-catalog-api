using Fcg.Catalog.Application._Shared.Contexts;
using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application.Properties;
using Fcg.Catalog.Domain.Promotions;
using MediatR;

namespace Fcg.Catalog.Application.Promotions.DeletePromotion;

internal sealed class DeletePromotionCommandHandler : IRequestHandler<DeletePromotionCommand>
{
    private readonly IPromotionRepository _promotions;
    private readonly ICatalogDbContext _context;

    public DeletePromotionCommandHandler(IPromotionRepository promotions, ICatalogDbContext context)
    {
        _promotions = promotions;
        _context = context;
    }

    public async Task Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await _promotions.GetByIdAsync(request.Id, cancellationToken)
                        ?? throw new NotFoundException(ApplicationResources.Promocao, request.Id);

        promotion.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
