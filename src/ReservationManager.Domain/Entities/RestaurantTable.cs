namespace ReservationManager.Domain.Entities;

public class RestaurantTable
{
    public Guid Id { get; private set; }
    public string TableNumber { get; private set; }
    public int Capacity { get; private set; }

    private RestaurantTable() { }

    public RestaurantTable(Guid id, string tableNumber, int capacity)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(tableNumber))
            throw new ArgumentException("Table number cannot be empty.", nameof(tableNumber));

        if (capacity < 2 || capacity > 10)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be between 2 and 10.");

        Id = id;
        TableNumber = tableNumber;
        Capacity = capacity;
    }
}
