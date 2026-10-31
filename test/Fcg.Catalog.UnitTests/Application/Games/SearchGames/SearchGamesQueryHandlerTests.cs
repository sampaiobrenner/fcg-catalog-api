using Fcg.Catalog.Application._Shared.ReadQueries;
using Fcg.Catalog.Application.Games.SearchGames;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Games.SearchGames;

public class SearchGamesQueryHandlerTests
{
    private readonly Mock<IGameReadQueries> _readQueries = new();

    [Fact]
    public async Task Handle_DeveNormalizarPaginacaoEFiltros_QuandoValoresForaDosLimites()
    {
        SearchGamesQuery? captured = null;
        _readQueries
            .Setup(r => r.SearchCatalogAsync(It.IsAny<SearchGamesQuery>(), It.IsAny<CancellationToken>()))
            .Callback<SearchGamesQuery, CancellationToken>((query, _) => captured = query)
            .ReturnsAsync([]);

        await new SearchGamesQueryHandler(_readQueries.Object).Handle(new SearchGamesQuery(" ", " RPG ", Page: 0, PageSize: 500), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Title.Should().BeNull();
        captured.Genre.Should().Be("RPG");
        captured.Page.Should().Be(SearchGamesQuery.DefaultPage);
        captured.PageSize.Should().Be(SearchGamesQuery.DefaultPageSize);
    }

    [Fact]
    public async Task Handle_DeveRetornarItensDaConsulta()
    {
        var item = new GameCatalogItemResponse { Id = Guid.CreateVersion7(), Title = "Hades", Price = 49.90m, EffectivePrice = 49.90m };
        _readQueries
            .Setup(r => r.SearchCatalogAsync(It.IsAny<SearchGamesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([item]);

        var result = await new SearchGamesQueryHandler(_readQueries.Object).Handle(new SearchGamesQuery(), CancellationToken.None);

        result.Should().ContainSingle().Which.Should().Be(item);
    }

    [Fact]
    public void Validator_DeveRejeitar_QuandoPrecoMaximoMenorQueMinimo()
    {
        var result = new SearchGamesQueryValidator().Validate(new SearchGamesQuery(MinPrice: 50, MaxPrice: 10));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(SearchGamesQuery.MaxPrice));
    }
}
