using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Application.DTOs;

namespace ReservationManager.Application.Features.Tables.Queries.GetAllTables;

public class GetAllTablesQueryHandler : IRequestHandler<GetAllTablesQuery, List<TableDto>>
{
    private readonly ITableRepository _tableRepository;

    public GetAllTablesQueryHandler(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<List<TableDto>> Handle(GetAllTablesQuery request, CancellationToken cancellationToken)
    {
        var tables = await _tableRepository.GetAllAsync();

        return tables
            .Select(t => new TableDto(t.Id, t.Name, t.Capacity))
            .ToList();
    }
}
