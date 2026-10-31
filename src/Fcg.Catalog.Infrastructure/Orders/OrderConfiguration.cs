using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Catalog.Infrastructure.Orders;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<OrderPersistenceModel>
{
    public void Configure(EntityTypeBuilder<OrderPersistenceModel> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();

        builder.Property(order => order.UserId).IsRequired();
        builder.Property(order => order.GameId).IsRequired();
        builder.Property(order => order.Price).IsRequired().HasColumnType("numeric(10,2)");
        builder.Property(order => order.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(order => order.Reason).HasMaxLength(Order.ReasonMaxLength);
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.UpdatedAt).IsRequired();

        // No maximo um pedido em andamento por usuario e jogo (protege contra cliques duplos e corridas).
        builder.HasIndex(order => new { order.UserId, order.GameId })
            .IsUnique()
            .HasFilter("status = 'Pending'")
            .HasDatabaseName("ux_orders_user_id_game_id_pending");

        builder.HasIndex(order => new { order.UserId, order.CreatedAt });

        builder.HasOne<GamePersistenceModel>()
            .WithMany()
            .HasForeignKey(order => order.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
