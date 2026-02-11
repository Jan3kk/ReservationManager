using Microsoft.EntityFrameworkCore;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Domain.Entities;
using ReservationManager.Infrastructure.Persistence;

namespace ReservationManager.Infrastructure.Repositories;

public class TableRepository : ITableRepository
{
    private readonly ReservationDbContext _context;

    public TableRepository(ReservationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> AddAsync(RestaurantTable table)
    {
        await _context.Tables.AddAsync(table);
        await _context.SaveChangesAsync();
        return table.Id;
    }

    public async Task<List<RestaurantTable>> GetAllAsync()
    {
        return await _context.Tables
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<RestaurantTable?> GetByIdAsync(Guid id)
    {
        return await _context.Tables
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<bool> ExistsByGuidAsync(Guid id)
    {
        return await _context.Tables
            .AnyAsync(t => t.Id == id);
    }

    public async Task<bool> IsUniqueNameTakenAsync(string uniqueName)
    {
        return await _context.Tables
            .AnyAsync(t => t.UniqueName == uniqueName);
    }

    public async Task DeleteAsync(Guid id)
    {
        var table = await _context.Tables.FindAsync(id);

        if (table is not null)
        {
            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
        }
    }
}
