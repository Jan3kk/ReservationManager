using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Domain.Entities;
using ReservationManager.Domain.Services;

namespace ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly ITableRepository _tableRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly AvailabilityService _availabilityService;

    public CreateReservationCommandHandler(
        ITableRepository tableRepository,
        IReservationRepository reservationRepository,
        AvailabilityService availabilityService)
    {
        _tableRepository = tableRepository;
        _reservationRepository = reservationRepository;
        _availabilityService = availabilityService;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var requestedDuration = TimeSpan.FromHours(request.DurationHours);

        var suitableTables = await _tableRepository.GetByCapacityAsync(request.PartySize);

        if (suitableTables.Count == 0)
        {
            throw new InvalidOperationException(
                "No suitable table available for the selected time and party size.");
        }

        var sortedTables = suitableTables
            .OrderBy(t => t.Capacity)
            .ToList();

        var tableIds = sortedTables
            .Select(t => t.Id)
            .ToList();

        var allReservations = await _reservationRepository.GetByTableIdsAndDateAsync(
            tableIds,
            request.Date);

        var reservationsByTable = allReservations.ToLookup(r => r.TableId);

        RestaurantTable? selectedTable = null;

        foreach (var table in sortedTables)
        {
            var existingReservations = reservationsByTable[table.Id].ToList();

            var availableStarts = _availabilityService.GetAvailableStartTimes(
                existingReservations,
                request.PartySize,
                table,
                request.Date,
                requestedDuration);

            if (availableStarts.Contains(request.StartTime))
            {
                selectedTable = table;
                break;
            }
        }

        if (selectedTable is null)
        {
            throw new InvalidOperationException(
                "No suitable table available for the selected time and party size.");
        }

        var reservationDateTime = request.Date.Date + request.StartTime;

        var reservation = new Reservation(
            id: Guid.NewGuid(),
            tableId: selectedTable.Id,
            customerName: request.CustomerName,
            customerEmail: request.CustomerEmail,
            customerPhone: request.CustomerPhone,
            reservationDate: reservationDateTime,
            durationHours: request.DurationHours,
            partySize: request.PartySize);

        var reservationId = await _reservationRepository.AddAsync(reservation);

        return reservationId;
    }
}
