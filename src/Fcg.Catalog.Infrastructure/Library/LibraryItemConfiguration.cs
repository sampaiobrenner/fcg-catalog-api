using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Catalog.Infrastructure.Library;

internal sealed class LibraryItemConfiguration : IEntityTypeConfiguration<LibraryItemPersistenceModel>
{
    public void Configure(EntityTypeBuilder<LibraryItemPersistenceModel> builder)
    {
        builder.ToTable("library_items");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();

        builder.Property(item => item.UserId).IsRequired();
        builder.Property(item => item.GameId).IsRequired();
        builder.Property(item => item.PricePaid).IsRequired().HasColumnType("numeric(10,2)");
        builder.Property(item => item.AcquiredAt).IsRequired();

        // Um usuario possui cada jogo no maximo uma vez (protege a conclusao da compra contra duplicidade).
        builder.HasIndex(item => new { item.UserId, item.GameId }).IsUnique();

        builder.HasOne<GamePersistenceModel>()
            .WithMany()
            .HasForeignKey(item => item.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
