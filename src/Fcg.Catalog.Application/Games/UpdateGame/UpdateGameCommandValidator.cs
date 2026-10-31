using FluentValidation;

namespace Fcg.Catalog.Application.Games.UpdateGame;

internal sealed class UpdateGameCommandValidator : AbstractValidator<UpdateGameCommand>
{
    public UpdateGameCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().MaximumLength(GameRules.TitleMaxLength);
        RuleFor(command => command.Genre).NotEmpty().MaximumLength(GameRules.GenreMaxLength);
        RuleFor(command => command.Description).MaximumLength(GameRules.DescriptionMaxLength);
        RuleFor(command => command.Price).GreaterThanOrEqualTo(0).LessThanOrEqualTo(GameRules.MaxPrice);
    }
}
