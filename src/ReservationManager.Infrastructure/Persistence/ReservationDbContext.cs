using Microsoft.EntityFrameworkCore;
using ReservationManager.Domain.Entities;

namespace ReservationManager.Infrastructure.Persistence;

public class ReservationDbContext : DbContext
{
    public ReservationDbContext(DbContextOptions<ReservationDbContext> options) : base(options)
    {
    }

    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<RestaurantTable> Tables { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationDbContext).Assembly);
    }
}
