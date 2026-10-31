using FluentValidation;

namespace Fcg.Catalog.Application.Orders.CompletePurchase;

internal sealed class CompletePurchaseCommandValidator : AbstractValidator<CompletePurchaseCommand>
{
    public CompletePurchaseCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.Status).IsInEnum();
    }
}
