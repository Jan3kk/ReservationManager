using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.Abstractions.Repositories;

public interface IReservationRepository
{
    Task<Guid> AddAsync(Reservation reservation);
    Task<Reservation?> GetByIdAsync(Guid id);
    Task<List<Reservation>> GetByTableAndDateAsync(Guid tableId, DateTime date);
    Task<bool> IsOverlapAsync(Guid tableId, DateTime start, DateTime end);
}
