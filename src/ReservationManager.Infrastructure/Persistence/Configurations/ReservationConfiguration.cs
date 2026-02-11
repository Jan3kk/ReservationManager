using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationManager.Domain.Entities;

namespace ReservationManager.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(r => r.TableId)
            .HasColumnName("table_id")
            .IsRequired();

        builder.Property(r => r.CustomerName)
            .HasColumnName("customer_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.CustomerEmail)
            .HasColumnName("customer_email")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.CustomerPhone)
            .HasColumnName("customer_phone")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.ReservationDate)
            .HasColumnName("reservation_date")
            .IsRequired();

        builder.Property(r => r.DurationHours)
            .HasColumnName("duration_hours")
            .HasColumnType("real")
            .IsRequired();

        builder.Property(r => r.PartySize)
            .HasColumnName("party_size")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);
    }
}
