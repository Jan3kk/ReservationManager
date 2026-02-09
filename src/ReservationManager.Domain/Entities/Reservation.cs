namespace ReservationManager.Domain.Entities;

public class Reservation
{
    public Guid Id { get; private set; }
    public Guid TableId { get; private set; }
    public string CustomerName { get; private set; }
    public string CustomerEmail { get; private set; }
    public string CustomerPhone { get; private set; }
    public DateTime ReservationDate { get; private set; }
    public int DurationHours { get; private set; }
    public ReservationStatus Status { get; private set; }

    private Reservation() { }

    public Reservation(
        Guid id,
        Guid tableId,
        string customerName,
        string customerEmail,
        string customerPhone,
        DateTime reservationDate,
        int durationHours)
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

        if (reservationDate <= DateTime.UtcNow)
            throw new ArgumentException("Reservation date must be in the future.", nameof(reservationDate));

        if (durationHours < 1 || durationHours > 3)
            throw new ArgumentOutOfRangeException(nameof(durationHours), "Duration must be between 1 and 3 hours.");

        if ((durationHours * 2) % 1 != 0)
            throw new ArgumentException("Duration must be in 0.5 hour increments.", nameof(durationHours));

        Id = id;
        TableId = tableId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        ReservationDate = reservationDate;
        DurationHours = durationHours;
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
