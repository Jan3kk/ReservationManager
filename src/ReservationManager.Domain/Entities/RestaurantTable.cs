namespace ReservationManager.Domain.Entities;

public class RestaurantTable
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int Capacity { get; private set; }

    private RestaurantTable() { }

    public RestaurantTable(Guid id, string name, int capacity)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        if (capacity < 2 || capacity > 10)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be between 2 and 10.");

        Id = id;
        Name = name;
        Capacity = capacity;
    }
}
