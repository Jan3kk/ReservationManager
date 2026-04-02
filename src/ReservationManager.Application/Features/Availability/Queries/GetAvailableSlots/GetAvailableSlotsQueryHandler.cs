using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Domain.Services;

namespace ReservationManager.Application.Features.Availability.Queries.GetAvailableSlots;

public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, List<TimeSpan>>
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

    public async Task<List<TimeSpan>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var requestedDuration = TimeSpan.FromHours(request.DurationHours);

        var suitableTables = await _tableRepository.GetByCapacityAsync(request.PartySize);

        if (suitableTables.Count == 0)
            return [];

        var tableIds = suitableTables
            .Select(t => t.Id)
            .ToList();

        var allReservations = await _reservationRepository.GetByTableIdsAndDateAsync(
            tableIds,
            request.Date);

        var reservationsByTable = allReservations.ToLookup(r => r.TableId);

        var uniqueStarts = new HashSet<TimeSpan>();

        foreach (var table in suitableTables)
        {
            var existingReservations = reservationsByTable[table.Id].ToList();

            var availableStarts = _availabilityService.GetAvailableStartTimes(
                existingReservations,
                request.PartySize,
                table,
                request.Date,
                requestedDuration);

            foreach (var start in availableStarts)
            {
                uniqueStarts.Add(start);
            }
        }

        return uniqueStarts
            .OrderBy(s => s)
            .ToList();
    }
}
