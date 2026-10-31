using Fcg.Catalog.Application.Properties;
using FluentValidation;

namespace Fcg.Catalog.Application.Promotions.CreatePromotion;

internal sealed class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public const int NameMaxLength = 120;

    public CreatePromotionCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(NameMaxLength);
        RuleFor(command => command.DiscountPercent).InclusiveBetween(0.01m, 100m);
        RuleFor(command => command.EndsAt)
            .GreaterThan(command => command.StartsAt)
            .WithMessage(ApplicationResources.DataDeTerminoInvalida);
        RuleFor(command => command.GameIds)
            .NotEmpty()
            .WithMessage(ApplicationResources.PromocaoSemJogos);
        RuleForEach(command => command.GameIds).NotEmpty();
    }
}
