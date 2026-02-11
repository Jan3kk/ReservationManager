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
        var isUniqueNameTaken = await _tableRepository.IsUniqueNameTakenAsync(request.UniqueName);

        if (isUniqueNameTaken)
        {
            throw new InvalidOperationException($"A table with unique name '{request.UniqueName}' already exists.");
        }

        var table = new RestaurantTable(
            id: Guid.NewGuid(),
            uniqueName: request.UniqueName,
            label: request.Label,
            capacity: request.Capacity);

        var tableId = await _tableRepository.AddAsync(table);

        return tableId;
    }
}
