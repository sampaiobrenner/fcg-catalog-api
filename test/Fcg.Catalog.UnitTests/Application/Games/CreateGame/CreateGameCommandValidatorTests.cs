using Fcg.Catalog.Application.Games.CreateGame;

namespace Fcg.Catalog.UnitTests.Application.Games.CreateGame;

public class CreateGameCommandValidatorTests
{
    private readonly CreateGameCommandValidator _validator = new();

    [Fact]
    public void Validate_DeveAceitar_QuandoDadosValidos()
    {
        _validator.Validate(new CreateGameCommand("Portal 2", "Puzzle", "Puzzle", 39.90m, null)).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Puzzle", 10, "Title")]
    [InlineData("Portal", "", 10, "Genre")]
    [InlineData("Portal", "Puzzle", -1, "Price")]
    public void Validate_DeveRejeitar_QuandoCampoInvalido(string title, string genre, decimal price, string property)
    {
        var result = _validator.Validate(new CreateGameCommand(title, null, genre, price, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == property);
    }

    [Fact]
    public void Validate_DeveRejeitar_QuandoTituloMaiorQue200()
    {
        var result = _validator.Validate(new CreateGameCommand(new string('a', 201), null, "Puzzle", 10m, null));

        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateGameCommand.Title));
    }
}
