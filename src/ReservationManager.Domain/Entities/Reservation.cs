using ReservationManager.Domain.Settings;

namespace ReservationManager.Domain.Entities;

public class Reservation
{
    public Guid Id { get; private set; }
    public Guid TableId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public DateTime ReservationDate { get; private set; }
    public float DurationHours { get; private set; }
    public int PartySize { get; private set; }
    public ReservationStatus Status { get; private set; }

    private Reservation() { }

    public Reservation(
        Guid id,
        Guid tableId,
        string customerName,
        string customerEmail,
        string customerPhone,
        DateTime reservationDate,
        float durationHours,
        int partySize)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (tableId == Guid.Empty)
            throw new ArgumentException("Table Id cannot be empty.", nameof(tableId));

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name cannot be empty.", nameof(customerName));

        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Customer email cannot be empty.", nameof(customerEmail));

        if (string.IsNullOrWhiteSpace(customerPhone))
            throw new ArgumentException("Customer phone cannot be empty.", nameof(customerPhone));

        var utcReservationDate = NormalizeToUtc(reservationDate);

        if (utcReservationDate <= DateTime.UtcNow)
            throw new ArgumentException("Reservation date must be in the future.", nameof(reservationDate));

        var minDurationHours = (float)RestaurantSettings.MinBookingDuration.TotalHours;
        var maxDurationHours = (float)RestaurantSettings.MaxBookingDuration.TotalHours;

        if (durationHours < minDurationHours || durationHours > maxDurationHours)
            throw new ArgumentOutOfRangeException(
                nameof(durationHours),
                $"Duration must be between {minDurationHours} and {maxDurationHours} hours.");

        if (!IsDurationOnIncrement(durationHours))
            throw new ArgumentException(
                $"Duration must be in {RestaurantSettings.DurationIncrement.TotalMinutes}-minute increments.",
                nameof(durationHours));

        if (partySize < RestaurantSettings.MinPartySize || partySize > RestaurantSettings.MaxPartySize)
            throw new ArgumentOutOfRangeException(
                nameof(partySize),
                $"Party size must be between {RestaurantSettings.MinPartySize} and {RestaurantSettings.MaxPartySize}.");

        Id = id;
        TableId = tableId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        ReservationDate = utcReservationDate;
        DurationHours = durationHours;
        PartySize = partySize;
        Status = ReservationStatus.Pending;
    }

    public void Confirm()
    {
        if (Status == ReservationStatus.Rejected)
            throw new InvalidOperationException("Cannot confirm a rejected reservation.");

        Status = ReservationStatus.Confirmed;
    }

    public void Reject()
    {
        Status = ReservationStatus.Rejected;
    }

    private static DateTime NormalizeToUtc(DateTime reservationDate)
    {
        if (reservationDate.Kind == DateTimeKind.Utc)
            return reservationDate;

        if (reservationDate.Kind == DateTimeKind.Local)
            return reservationDate.ToUniversalTime();

        return DateTime.SpecifyKind(reservationDate, DateTimeKind.Utc);
    }

    private static bool IsDurationOnIncrement(float durationHours)
    {
        var durationMinutes = (double)durationHours * 60.0;
        var incrementMinutes = RestaurantSettings.DurationIncrement.TotalMinutes;
        var remainder = durationMinutes % incrementMinutes;

        return remainder < 1e-6 || Math.Abs(remainder - incrementMinutes) < 1e-6;
    }
}
