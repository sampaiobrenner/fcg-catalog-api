using FluentValidation;

namespace Fcg.Catalog.Application.Orders.PlaceOrder;

internal sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(command => command.GameId).NotEmpty();
    }
}
