namespace ReservationManager.Domain.Entities;

public class RestaurantTable
{
    public Guid Id { get; private set; }
    public string UniqueName { get; private set; }
    public string Label { get; private set; }
    public int Capacity { get; private set; }

    private RestaurantTable() { }

    public RestaurantTable(Guid id, string uniqueName, string label, int capacity)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(uniqueName))
            throw new ArgumentException("UniqueName cannot be empty.", nameof(uniqueName));

        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be empty.", nameof(label));

        if (capacity < 2 || capacity > 10)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be between 2 and 10.");

        Id = id;
        UniqueName = uniqueName;
        Label = label;
        Capacity = capacity;
    }
}
