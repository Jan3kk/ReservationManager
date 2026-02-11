namespace ReservationManager.Domain.Settings;

public static class RestaurantSettings
{
    public static TimeSpan OpenTime => new(12, 0, 0);

    public static TimeSpan CloseTime => new(22, 0, 0);

    public static TimeSpan MinBookingDuration => TimeSpan.FromHours(1.5);

    public static TimeSpan MaxBookingDuration => TimeSpan.FromHours(3.0);

    public static TimeSpan SlotInterval => TimeSpan.FromMinutes(30);

    public static TimeSpan DurationIncrement => TimeSpan.FromMinutes(30);

    public static TimeSpan MinAdvanceBookingTime => TimeSpan.FromHours(3);

    public static int MinPartySize => 1;

    public static int MaxPartySize => 10;
}
