using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Application.Exceptions;
using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly ITableRepository _tableRepository;
    private readonly IReservationRepository _reservationRepository;

    public CreateReservationCommandHandler(
        ITableRepository tableRepository,
        IReservationRepository reservationRepository)
    {
        _tableRepository = tableRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var suitableTables = await _tableRepository.GetByCapacityAsync(request.PartySize);

        if (suitableTables.Count == 0)
        {
            throw new ConflictException(
                "No suitable table available for the selected time and party size.");
        }

        var reservationStart = DateTime.SpecifyKind(
            request.Date.Date + request.StartTime,
            DateTimeKind.Utc);

        var reservationEnd = reservationStart.AddHours(request.DurationHours);

        RestaurantTable? selectedTable = null;

        foreach (var table in suitableTables.OrderBy(t => t.Capacity))
        {
            var hasOverlap = await _reservationRepository.IsOverlapAsync(table.Id, reservationStart, reservationEnd);

            if (!hasOverlap)
            {
                selectedTable = table;
                break;
            }
        }

        if (selectedTable is null)
        {
            throw new ConflictException(
                "This time slot is no longer available. Please check available slots and try again.");
        }

        var reservation = new Reservation(
            id: Guid.NewGuid(),
            tableId: selectedTable.Id,
            customerName: request.CustomerName,
            customerEmail: request.CustomerEmail,
            customerPhone: request.CustomerPhone,
            reservationDate: reservationStart,
            durationHours: request.DurationHours,
            partySize: request.PartySize);

        var reservationId = await _reservationRepository.AddAsync(reservation);

        return reservationId;
    }
}
