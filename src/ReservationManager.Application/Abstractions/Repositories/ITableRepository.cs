using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.Abstractions.Repositories;

public interface ITableRepository
{
    Task<Guid> AddAsync(RestaurantTable table);
    Task<List<RestaurantTable>> GetAllAsync();
    Task<RestaurantTable?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task DeleteAsync(Guid id);
}
