using FluentValidation;

namespace Fcg.Catalog.Application.Games.CreateGame;

internal sealed class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MaximumLength(GameRules.TitleMaxLength);
        RuleFor(command => command.Genre).NotEmpty().MaximumLength(GameRules.GenreMaxLength);
        RuleFor(command => command.Description).MaximumLength(GameRules.DescriptionMaxLength);
        RuleFor(command => command.Price).GreaterThanOrEqualTo(0).LessThanOrEqualTo(GameRules.MaxPrice);
    }
}
