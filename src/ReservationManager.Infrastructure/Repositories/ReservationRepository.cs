using Microsoft.EntityFrameworkCore;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Domain.Entities;
using ReservationManager.Infrastructure.Persistence;

namespace ReservationManager.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ReservationDbContext _context;

    public ReservationRepository(ReservationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> AddAsync(Reservation reservation)
    {
        await _context.Reservations.AddAsync(reservation);
        await _context.SaveChangesAsync();
        return reservation.Id;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id)
    {
        return await _context.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> IsOverlapAsync(Guid tableId, DateTime start, DateTime end)
    {
        var reservations = await _context.Reservations
            .Where(r => r.TableId == tableId && r.Status != ReservationStatus.Rejected)
            .Select(r => new
            {
                Start = r.ReservationDate,
                End = r.ReservationDate.AddHours(r.DurationHours)
            })
            .ToListAsync();

        var hasOverlap = reservations.Any(r => r.Start < end && r.End > start);

        return hasOverlap;
    }
}
