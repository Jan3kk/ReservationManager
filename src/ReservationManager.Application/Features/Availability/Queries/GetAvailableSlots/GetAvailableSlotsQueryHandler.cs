using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Application.DTOs;
using ReservationManager.Domain.Services;

namespace ReservationManager.Application.Features.Availability.Queries.GetAvailableSlots;

public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, List<TimeSlotDto>>
{
    private readonly ITableRepository _tableRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly AvailabilityService _availabilityService;

    public GetAvailableSlotsQueryHandler(
        ITableRepository tableRepository,
        IReservationRepository reservationRepository,
        AvailabilityService availabilityService)
    {
        _tableRepository = tableRepository;
        _reservationRepository = reservationRepository;
        _availabilityService = availabilityService;
    }

    public async Task<List<TimeSlotDto>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var suitableTables = await _tableRepository.GetByCapacityAsync(request.PartySize);

        if (suitableTables.Count == 0)
        {
            return [];
        }

        var tableIds = suitableTables
            .Select(t => t.Id)
            .ToList();

        var reservationDayUtc = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);

        var allReservations = await _reservationRepository.GetByTableIdsAndDateAsync(
            tableIds,
            reservationDayUtc);

        var reservationsByTable = allReservations.ToLookup(r => r.TableId);

        var uniqueSlots = new HashSet<TimeSlotDto>();

        foreach (var table in suitableTables)
        {
            var existingReservations = reservationsByTable[table.Id].ToList();

            var validSlots = _availabilityService.GetValidTimeSlots(
                existingReservations,
                request.PartySize,
                table,
                reservationDayUtc,
                request.DurationHours);

            foreach (var slot in validSlots)
            {
                uniqueSlots.Add(new TimeSlotDto(slot.Start, slot.End));
            }
        }

        return uniqueSlots
            .OrderBy(s => s.Start)
            .ToList();
    }
}
