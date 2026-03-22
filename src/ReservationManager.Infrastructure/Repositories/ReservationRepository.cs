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

    public async Task<List<Reservation>> GetByTableAndDateAsync(Guid tableId, DateTime date)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Where(r => r.TableId == tableId && r.ReservationDate.Date == date.Date)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetByTableIdsAndDateAsync(IEnumerable<Guid> tableIds, DateTime date)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Where(r => tableIds.Contains(r.TableId) && r.ReservationDate.Date == date.Date)
            .ToListAsync();
    }

    public async Task<bool> IsOverlapAsync(Guid tableId, DateTime start, DateTime end)
    {
        var dayStart = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        var reservationsForDay = await _context.Reservations
            .AsNoTracking()
            .Where(r =>
                r.TableId == tableId &&
                r.Status != ReservationStatus.Rejected &&
                r.ReservationDate >= dayStart &&
                r.ReservationDate < dayEnd)
            .Select(r => new { r.ReservationDate, r.DurationHours })
            .ToListAsync();

        return reservationsForDay.Any(r =>
            r.ReservationDate < end &&
            r.ReservationDate.AddHours(r.DurationHours) > start);
    }

    public async Task<bool> HasAnyForTableAsync(Guid tableId)
    {
        return await _context.Reservations.AnyAsync(r => r.TableId == tableId);
    }
}
