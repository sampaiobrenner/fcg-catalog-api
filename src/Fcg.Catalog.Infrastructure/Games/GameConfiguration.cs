using Fcg.Catalog.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Catalog.Infrastructure.Games;

internal sealed class GameConfiguration : IEntityTypeConfiguration<GamePersistenceModel>
{
    public void Configure(EntityTypeBuilder<GamePersistenceModel> builder)
    {
        builder.ToTable("games");

        builder.HasKey(game => game.Id);
        builder.Property(game => game.Id).ValueGeneratedNever();

        builder.Property(game => game.Title).IsRequired().HasMaxLength(200);
        builder.Property(game => game.Description).IsRequired().HasMaxLength(2000);
        builder.Property(game => game.Genre).IsRequired().HasMaxLength(80);
        builder.Property(game => game.Price).IsRequired().HasColumnType("numeric(10,2)");
        builder.Property(game => game.ReleaseDate).HasColumnType("date");
        builder.Property(game => game.Active).IsRequired();
        builder.Property(game => game.CreatedAt).IsRequired();

        builder.HasIndex(game => game.Title);
        builder.HasIndex(game => game.Genre);
    }
}
