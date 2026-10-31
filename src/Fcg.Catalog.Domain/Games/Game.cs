using Fcg.Catalog.Domain._Shared.Exceptions;
using Fcg.Catalog.Domain._Shared.Models;
using Fcg.Catalog.Domain._Shared.ValueObjects;
using Fcg.Catalog.Domain.Properties;

namespace Fcg.Catalog.Domain.Games;

/// <summary>Raiz de agregado: um jogo do catalogo.</summary>
public sealed class Game : DomainModelBase<GamePersistenceModel>, IAggregateRoot
{
    public Game(GamePersistenceModel persistenceModel) : base(persistenceModel)
    {
    }

    public string Title => PersistenceModel.Title;

    public string Description => PersistenceModel.Description;

    public string Genre => PersistenceModel.Genre;

    public Money Price => Money.Create(PersistenceModel.Price);

    public DateOnly? ReleaseDate => PersistenceModel.ReleaseDate;

    public bool Active => PersistenceModel.Active;

    public DateTime CreatedAt => PersistenceModel.CreatedAt;

    public static Game Create(string title, string? description, string genre, Money price, DateOnly? releaseDate)
    {
        var game = new Game(new GamePersistenceModel
        {
            Active = true,
            CreatedAt = DateTime.UtcNow
        });

        game.Update(title, description, genre, price, releaseDate);
        return game;
    }

    public void Update(string title, string? description, string genre, Money price, DateOnly? releaseDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessException(DomainResources.TituloDoJogoObrigatorio);

        if (string.IsNullOrWhiteSpace(genre))
            throw new BusinessException(DomainResources.GeneroDoJogoObrigatorio);

        ArgumentNullException.ThrowIfNull(price);

        PersistenceModel.Title = title.Trim();
        PersistenceModel.Description = description?.Trim() ?? string.Empty;
        PersistenceModel.Genre = genre.Trim();
        PersistenceModel.Price = price.Amount;
        PersistenceModel.ReleaseDate = releaseDate;
    }

    public void Activate() => PersistenceModel.Active = true;

    /// <summary>Desativa o jogo (soft delete), preservando bibliotecas e pedidos.</summary>
    public void Deactivate() => PersistenceModel.Active = false;
}
