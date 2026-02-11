using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Application.DTOs;

namespace ReservationManager.Application.Features.Tables.Queries.GetTableById;

public class GetTableByIdQueryHandler : IRequestHandler<GetTableByIdQuery, TableDto>
{
    private readonly ITableRepository _tableRepository;

    public GetTableByIdQueryHandler(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<TableDto> Handle(GetTableByIdQuery request, CancellationToken cancellationToken)
    {
        var table = await _tableRepository.GetByIdAsync(request.Id);

        if (table is null)
        {
            throw new KeyNotFoundException($"Table with ID '{request.Id}' was not found.");
        }

        return new TableDto(table.Id, table.UniqueName, table.Label, table.Capacity);
    }
}
