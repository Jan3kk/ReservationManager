using MediatR;
using ReservationManager.Application.Abstractions.Repositories;

namespace ReservationManager.Application.Features.Tables.Commands.DeleteTable;

public class DeleteTableCommandHandler : IRequestHandler<DeleteTableCommand>
{
    private readonly ITableRepository _tableRepository;

    public DeleteTableCommandHandler(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task Handle(DeleteTableCommand request, CancellationToken cancellationToken)
    {
        var exists = await _tableRepository.ExistsByGuidAsync(request.Id);

        if (!exists)
        {
            throw new InvalidOperationException($"Table with ID '{request.Id}' does not exist.");
        }

        await _tableRepository.DeleteAsync(request.Id);
    }
}
