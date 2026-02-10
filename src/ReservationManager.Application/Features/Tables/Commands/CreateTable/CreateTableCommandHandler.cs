using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.Features.Tables.Commands.CreateTable;

public class CreateTableCommandHandler : IRequestHandler<CreateTableCommand, Guid>
{
    private readonly ITableRepository _tableRepository;

    public CreateTableCommandHandler(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<Guid> Handle(CreateTableCommand request, CancellationToken cancellationToken)
    {
        var table = new RestaurantTable(
            id: Guid.NewGuid(),
            name: request.Name,
            capacity: request.Capacity);

        var tableId = await _tableRepository.AddAsync(table);

        return tableId;
    }
}
