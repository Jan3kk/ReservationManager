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

        var minDuration = (float)RestaurantSettings.MinBookingDuration.TotalHours;
        var maxDuration = (float)RestaurantSettings.MaxBookingDuration.TotalHours;
        if (durationHours < minDuration || durationHours > maxDuration)
            throw new ArgumentOutOfRangeException(nameof(durationHours),
                $"Duration must be between {minDuration} and {maxDuration} hours.");

        var incrementMinutes = RestaurantSettings.DurationIncrement.TotalMinutes;
        if (durationHours * 60 % incrementMinutes != 0)
            throw new ArgumentException(
                $"Duration must be in {incrementMinutes}-minute increments.", nameof(durationHours));

        if (partySize < RestaurantSettings.MinPartySize || partySize > RestaurantSettings.MaxPartySize)
            throw new ArgumentOutOfRangeException(nameof(partySize),
                $"Party size must be between {RestaurantSettings.MinPartySize} and {RestaurantSettings.MaxPartySize}.");

        Id = id;
        TableId = tableId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        ReservationDate = reservationDate;
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
}
