using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Catalog.Infrastructure.Promotions;

internal sealed class PromotionGameConfiguration : IEntityTypeConfiguration<PromotionGamePersistenceModel>
{
    public void Configure(EntityTypeBuilder<PromotionGamePersistenceModel> builder)
    {
        builder.ToTable("promotion_games");

        builder.HasKey(promotionGame => promotionGame.Id);
        builder.Property(promotionGame => promotionGame.Id).ValueGeneratedNever();

        builder.Property(promotionGame => promotionGame.PromotionId).IsRequired();
        builder.Property(promotionGame => promotionGame.GameId).IsRequired();

        builder.HasIndex(promotionGame => new { promotionGame.PromotionId, promotionGame.GameId }).IsUnique();

        builder.HasOne<GamePersistenceModel>()
            .WithMany()
            .HasForeignKey(promotionGame => promotionGame.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
