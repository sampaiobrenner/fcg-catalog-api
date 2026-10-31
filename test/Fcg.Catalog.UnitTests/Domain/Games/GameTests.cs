using Fcg.Catalog.Domain._Shared.Exceptions;
using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Games;

namespace Fcg.Catalog.UnitTests.Domain.Games;

public class GameTests
{
    [Fact]
    public void Create_DeveCriarJogoAtivo_QuandoDadosValidos()
    {
        var game = Game.Create("  Portal 2 ", " Puzzle ", " Puzzle ", Money.Create(39.90m), new DateOnly(2011, 4, 18));

        game.Id().Should().NotBeEmpty();
        game.Title.Should().Be("Portal 2");
        game.Description.Should().Be("Puzzle");
        game.Genre.Should().Be("Puzzle");
        game.Price.Amount.Should().Be(39.90m);
        game.Active.Should().BeTrue();
        game.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("", "Puzzle", "título")]
    [InlineData("Portal 2", " ", "gênero")]
    public void Create_DeveLancarExcecao_QuandoCampoObrigatorioAusente(string title, string genre, string expected)
    {
        var act = () => Game.Create(title, "desc", genre, Money.Create(10m), null);

        act.Should().Throw<BusinessException>().WithMessage($"*{expected}*");
    }

    [Fact]
    public void Create_DeveUsarDescricaoVazia_QuandoDescricaoNula()
    {
        Game.Create("Portal", null, "Puzzle", Money.Create(10m), null).Description.Should().BeEmpty();
    }

    [Fact]
    public void Update_DeveAlterarDadosMutaveis()
    {
        var game = Game.Create("Portal", "Puzzle", "Puzzle", Money.Create(19.90m), null);

        game.Update("Portal 2", "Sequel", "Puzzle-Platformer", Money.Create(39.90m), new DateOnly(2011, 4, 18));

        game.Title.Should().Be("Portal 2");
        game.Description.Should().Be("Sequel");
        game.Genre.Should().Be("Puzzle-Platformer");
        game.Price.Amount.Should().Be(39.90m);
        game.ReleaseDate.Should().Be(new DateOnly(2011, 4, 18));
    }

    [Fact]
    public void DeactivateEActivate_DevemAlternarStatus()
    {
        var game = Game.Create("Portal", "Puzzle", "Puzzle", Money.Create(19.90m), null);

        game.Deactivate();
        game.Active.Should().BeFalse();

        game.Activate();
        game.Active.Should().BeTrue();
    }
}
