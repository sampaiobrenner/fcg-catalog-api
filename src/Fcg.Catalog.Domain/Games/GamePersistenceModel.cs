using Fcg.Catalog.Domain._Shared.Models;

namespace Fcg.Catalog.Domain.Games;

public sealed class GamePersistenceModel : PersistenceModelBase
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}
