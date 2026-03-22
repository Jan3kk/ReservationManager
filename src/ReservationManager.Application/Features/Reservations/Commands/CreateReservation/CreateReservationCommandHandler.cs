using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Application.Exceptions;
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
            throw new ConflictException(
                "No suitable table available for the selected time and party size.");
        }

        var sortedTables = suitableTables
            .OrderBy(t => t.Capacity)
            .ToList();

        var tableIds = sortedTables
            .Select(t => t.Id)
            .ToList();

        var reservationDayUtc = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);

        var allReservations = await _reservationRepository.GetByTableIdsAndDateAsync(
            tableIds,
            reservationDayUtc);

        var reservationsByTable = allReservations.ToLookup(r => r.TableId);

        var requestedSlotStart = request.StartTime;
        var requestedSlotEnd = request.StartTime + TimeSpan.FromHours(request.DurationHours);

        RestaurantTable? selectedTable = null;

        foreach (var table in sortedTables)
        {
            var existingReservations = reservationsByTable[table.Id].ToList();

            var validSlots = _availabilityService.GetValidTimeSlots(
                existingReservations,
                request.PartySize,
                table,
                reservationDayUtc,
                request.DurationHours);

            var isValidStartTime = validSlots.Any(slot =>
                slot.Start == requestedSlotStart && slot.End == requestedSlotEnd);

            if (!isValidStartTime)
            {
                continue;
            }

            var hasOverlap = existingReservations
                .Where(r => r.Status != ReservationStatus.Rejected)
                .Any(r =>
                {
                    var existingStart = r.ReservationDate.TimeOfDay;
                    var existingEnd = existingStart + TimeSpan.FromHours(r.DurationHours);

                    return existingStart < requestedSlotEnd && existingEnd > requestedSlotStart;
                });

            if (hasOverlap)
            {
                continue;
            }

            selectedTable = table;
            break;
        }

        if (selectedTable is null)
        {
            throw new ConflictException(
                "No suitable table available for the selected time and party size.");
        }

        var reservationDateTime = DateTime.SpecifyKind(
            request.Date.Date + request.StartTime,
            DateTimeKind.Utc);

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
