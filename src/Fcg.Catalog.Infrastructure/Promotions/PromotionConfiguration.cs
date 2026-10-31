using Fcg.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Catalog.Infrastructure.Promotions;

internal sealed class PromotionConfiguration : IEntityTypeConfiguration<PromotionPersistenceModel>
{
    public void Configure(EntityTypeBuilder<PromotionPersistenceModel> builder)
    {
        builder.ToTable("promotions");

        builder.HasKey(promotion => promotion.Id);
        builder.Property(promotion => promotion.Id).ValueGeneratedNever();

        builder.Property(promotion => promotion.Name).IsRequired().HasMaxLength(120);
        builder.Property(promotion => promotion.DiscountPercent).IsRequired().HasColumnType("numeric(5,2)");
        builder.Property(promotion => promotion.StartsAt).IsRequired();
        builder.Property(promotion => promotion.EndsAt).IsRequired();
        builder.Property(promotion => promotion.Active).IsRequired();
        builder.Property(promotion => promotion.CreatedAt).IsRequired();

        builder.HasMany(promotion => promotion.Games)
            .WithOne()
            .HasForeignKey(promotionGame => promotionGame.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
