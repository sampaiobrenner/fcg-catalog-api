using Fcg.Catalog.Application.Games.SearchGames;
using Fcg.Catalog.Application.Library.GetMyLibrary;

namespace Fcg.Catalog.Application._Shared.ReadQueries;

/// <summary>
/// Consultas de leitura de alta performance (Dapper na infraestrutura): catalogo com preco
/// promocional efetivo e biblioteca do usuario, sem o overhead do change tracking do EF.
/// </summary>
public interface IGameReadQueries
{
    Task<IReadOnlyCollection<GameCatalogItemResponse>> SearchCatalogAsync(SearchGamesQuery filter, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<LibraryItemResponse>> GetUserLibraryAsync(Guid userId, CancellationToken cancellationToken);
}
