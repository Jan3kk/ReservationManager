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
        var suitableTables = await _tableRepository.GetByCapacityAsync(request.PartySize);

        if (suitableTables.Count == 0)
        {
            throw new InvalidOperationException(
                "No suitable table available for the selected time and party size.");
        }

        var sortedTables = suitableTables
            .OrderBy(t => t.Capacity)
            .ToList();

        var requestedSlotEnd = request.StartTime + TimeSpan.FromHours(request.DurationHours);

        RestaurantTable? selectedTable = null;

        foreach (var table in sortedTables)
        {
            var existingReservations = await _reservationRepository.GetByTableAndDateAsync(
                table.Id,
                request.Date);

            var validSlots = _availabilityService.GetValidTimeSlots(
                existingReservations,
                request.PartySize,
                table,
                request.Date);

            var matchingSlot = validSlots.FirstOrDefault(slot =>
                slot.Start <= request.StartTime && 
                requestedSlotEnd <= slot.End + TimeSpan.FromHours(request.DurationHours - (float)slot.Duration.TotalHours));

            var isValidStartTime = validSlots.Any(slot => slot.Start == request.StartTime);

            if (isValidStartTime)
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
            durationHours: request.DurationHours);

        var reservationId = await _reservationRepository.AddAsync(reservation);

        return reservationId;
    }
}
