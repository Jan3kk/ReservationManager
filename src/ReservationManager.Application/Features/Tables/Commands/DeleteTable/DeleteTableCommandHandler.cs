using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Application.Exceptions;

namespace ReservationManager.Application.Features.Tables.Commands.DeleteTable;

public class DeleteTableCommandHandler : IRequestHandler<DeleteTableCommand>
{
    private readonly ITableRepository _tableRepository;
    private readonly IReservationRepository _reservationRepository;

    public DeleteTableCommandHandler(
        ITableRepository tableRepository,
        IReservationRepository reservationRepository)
    {
        _tableRepository = tableRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task Handle(DeleteTableCommand request, CancellationToken cancellationToken)
    {
        var exists = await _tableRepository.ExistsByGuidAsync(request.Id);

        if (!exists)
        {
            throw new NotFoundException($"Table with ID '{request.Id}' was not found.");
        }

        if (await _reservationRepository.HasAnyForTableAsync(request.Id))
        {
            throw new ConflictException("Cannot delete a table that has reservations.");
        }

        await _tableRepository.DeleteAsync(request.Id);
    }
}
