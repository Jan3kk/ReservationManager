using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.Abstractions.Repositories;

public interface IReservationRepository
{
    Task<Guid> AddAsync(Reservation reservation);
    Task<Reservation?> GetByIdAsync(Guid id);
    Task<bool> IsOverlapAsync(Guid tableId, DateTime start, DateTime end);
}
