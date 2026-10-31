namespace Fcg.Catalog.Application.Games;

/// <summary>Limites de tamanho alinhados as colunas da tabela <c>games</c>.</summary>
internal static class GameRules
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int GenreMaxLength = 80;

    /// <summary>Maior valor que cabe em <c>numeric(10,2)</c>.</summary>
    public const decimal MaxPrice = 99_999_999.99m;
}
