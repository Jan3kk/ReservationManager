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
        if (request.DurationHours <= 0)
        {
            throw new ArgumentException("Duration must be greater than 0.", nameof(request.DurationHours));
        }

        if (request.PartySize <= 0)
        {
            throw new ArgumentException("Party size must be greater than 0.", nameof(request.PartySize));
        }

        var suitableTables = await _tableRepository.GetByCapacityAsync(request.PartySize);

        if (suitableTables.Count == 0)
        {
            return [];
        }

        var tableIds = suitableTables
            .Select(t => t.Id)
            .ToList();

        var allReservations = await _reservationRepository.GetByTableIdsAndDateAsync(
            tableIds,
            request.Date);

        var reservationsByTable = allReservations.ToLookup(r => r.TableId);

        var uniqueSlots = new HashSet<TimeSlotDto>();

        foreach (var table in suitableTables)
        {
            var existingReservations = reservationsByTable[table.Id].ToList();

            var validSlots = _availabilityService.GetValidTimeSlots(
                existingReservations,
                request.PartySize,
                table,
                request.Date);

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
