using ReservationManager.Domain.Entities;
using ReservationManager.Domain.Models;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Domain.Services;

public class AvailabilityService
{
    private record ReservationWindow(TimeSpan Start, TimeSpan End);

    public List<TimeSlot> GetValidTimeSlots(
        List<Reservation> existingReservations,
        int partySize,
        RestaurantTable table,
        DateTime date,
        float durationHours)
    {
        if (table.Capacity < partySize)
        {
            return [];
        }

        var bookingDuration = TimeSpan.FromHours(durationHours);

        var validSlots = new List<TimeSlot>();

        var reservationsForDate = existingReservations
            .Where(r => r.ReservationDate.Date == date.Date
                && r.Status != ReservationStatus.Rejected)
            .ToList();

        var reservationWindows = reservationsForDate
            .Select(r => new ReservationWindow(
                r.ReservationDate.TimeOfDay,
                r.ReservationDate.TimeOfDay + TimeSpan.FromHours(r.DurationHours)))
            .ToList();

        var currentTime = RestaurantSettings.OpenTime;

        while (currentTime + bookingDuration <= RestaurantSettings.CloseTime)
        {
            var slotDateTime = DateTime.SpecifyKind(date.Date + currentTime, DateTimeKind.Utc);
            var minAllowedDateTime = DateTime.UtcNow.Add(RestaurantSettings.MinAdvanceBookingTime);

            if (slotDateTime < minAllowedDateTime)
            {
                currentTime += RestaurantSettings.SlotInterval;
                continue;
            }

            var candidateEnd = currentTime + bookingDuration;

            if (IsSlotValid(currentTime, candidateEnd, reservationWindows))
            {
                validSlots.Add(new TimeSlot(currentTime, candidateEnd));
            }

            currentTime += RestaurantSettings.SlotInterval;
        }

        return validSlots;
    }

    private static bool IsSlotValid(
        TimeSpan slotStart,
        TimeSpan slotEnd,
        List<ReservationWindow> reservationWindows)
    {
        foreach (var reservation in reservationWindows)
        {
            if (slotStart < reservation.End && slotEnd > reservation.Start)
            {
                return false;
            }
        }

        var previousReservation = reservationWindows
            .Where(r => r.End <= slotStart)
            .OrderByDescending(r => r.End)
            .FirstOrDefault();

        if (previousReservation is not null)
        {
            var gapBefore = slotStart - previousReservation.End;

            if (gapBefore > TimeSpan.Zero && gapBefore < RestaurantSettings.MinBookingDuration)
            {
                return false;
            }
        }

        var nextReservation = reservationWindows
            .Where(r => r.Start >= slotEnd)
            .OrderBy(r => r.Start)
            .FirstOrDefault();

        if (nextReservation is not null)
        {
            var gapAfter = nextReservation.Start - slotEnd;

            if (gapAfter > TimeSpan.Zero && gapAfter < RestaurantSettings.MinBookingDuration)
            {
                return false;
            }
        }
        else
        {
            var gapToClose = RestaurantSettings.CloseTime - slotEnd;

            if (gapToClose > TimeSpan.Zero && gapToClose < RestaurantSettings.MinBookingDuration)
            {
                return false;
            }
        }

        return true;
    }
}
