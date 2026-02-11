using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.Abstractions.Repositories;

public interface ITableRepository
{
    Task<Guid> AddAsync(RestaurantTable table);
    Task<List<RestaurantTable>> GetAllAsync();
    Task<RestaurantTable?> GetByIdAsync(Guid id);
    Task<List<RestaurantTable>> GetByCapacityAsync(int minCapacity);
    Task<bool> ExistsByGuidAsync(Guid id);
    Task<bool> IsUniqueNameTakenAsync(string uniqueName);
    Task DeleteAsync(Guid id);
}
