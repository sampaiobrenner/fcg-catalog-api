using Fcg.Catalog.Application.Properties;
using FluentValidation;

namespace Fcg.Catalog.Application.Games.SearchGames;

internal sealed class SearchGamesQueryValidator : AbstractValidator<SearchGamesQuery>
{
    public SearchGamesQueryValidator()
    {
        RuleFor(query => query.MinPrice).GreaterThanOrEqualTo(0).When(query => query.MinPrice.HasValue);
        RuleFor(query => query.MaxPrice).GreaterThanOrEqualTo(0).When(query => query.MaxPrice.HasValue);
        RuleFor(query => query.MaxPrice)
            .GreaterThanOrEqualTo(query => query.MinPrice)
            .When(query => query.MinPrice.HasValue && query.MaxPrice.HasValue)
            .WithMessage(ApplicationResources.PrecoMaximoMenorQueMinimo);
    }
}
