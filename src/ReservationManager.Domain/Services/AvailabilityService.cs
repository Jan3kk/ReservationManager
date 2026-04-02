using ReservationManager.Domain.Entities;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Domain.Services;

public class AvailabilityService
{
    private record OccupiedBlock(TimeSpan Start, TimeSpan End);

    private record FreeWindow(TimeSpan Start, TimeSpan End, bool EndsAtReservation);

    public List<TimeSpan> GetAvailableStartTimes(
        List<Reservation> existingReservations,
        int partySize,
        RestaurantTable table,
        DateTime date,
        TimeSpan requestedDuration)
    {
        if (table.Capacity < partySize)
            return [];

        var occupiedBlocks = existingReservations
            .Where(r => r.ReservationDate.Date == date.Date
                        && r.Status != ReservationStatus.Rejected)
            .Select(r => new OccupiedBlock(
                r.ReservationDate.TimeOfDay,
                r.ReservationDate.TimeOfDay + TimeSpan.FromHours(r.DurationHours)))
            .OrderBy(b => b.Start)
            .ToList();

        var freeWindows = BuildFreeWindows(occupiedBlocks);
        var gridStep = RestaurantSettings.MinBookingDuration + RestaurantSettings.BufferTime;
        var minGapAfter = RestaurantSettings.BufferTime + RestaurantSettings.MinBookingDuration;
        var availableStarts = new List<TimeSpan>();

        foreach (var window in freeWindows)
        {
            var deadline = window.EndsAtReservation
                ? window.End - RestaurantSettings.BufferTime
                : window.End;

            var candidate = window.Start;

            while (candidate + requestedDuration <= deadline)
            {
                var remaining = deadline - (candidate + requestedDuration);

                if (remaining == TimeSpan.Zero || remaining >= minGapAfter)
                {
                    var slotDateTime = date.Date + candidate;
                    var minAllowed = DateTime.UtcNow + RestaurantSettings.MinAdvanceBookingTime;

                    if (slotDateTime >= minAllowed)
                        availableStarts.Add(candidate);
                }

                candidate += gridStep;
            }
        }

        return availableStarts;
    }

    private static List<FreeWindow> BuildFreeWindows(List<OccupiedBlock> occupiedBlocks)
    {
        var windows = new List<FreeWindow>();
        var windowStart = RestaurantSettings.OpenTime;

        foreach (var block in occupiedBlocks)
        {
            if (block.Start > windowStart)
                windows.Add(new FreeWindow(windowStart, block.Start, EndsAtReservation: true));

            var blockEnd = block.End + RestaurantSettings.BufferTime;
            if (blockEnd > windowStart)
                windowStart = blockEnd;
        }

        if (windowStart < RestaurantSettings.CloseTime)
            windows.Add(new FreeWindow(windowStart, RestaurantSettings.CloseTime, EndsAtReservation: false));

        return windows;
    }
}
