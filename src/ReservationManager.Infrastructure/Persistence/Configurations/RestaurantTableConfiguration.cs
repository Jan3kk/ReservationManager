using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationManager.Domain.Entities;

namespace ReservationManager.Infrastructure.Persistence.Configurations;

public class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
{
    public void Configure(EntityTypeBuilder<RestaurantTable> builder)
    {
        builder.ToTable("restaurant_tables");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.UniqueName)
            .HasColumnName("unique_name")
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.UniqueName)
            .IsUnique();

        builder.Property(t => t.Label)
            .HasColumnName("label")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Capacity)
            .HasColumnName("capacity")
            .IsRequired();
    }
}
