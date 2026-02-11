namespace ReservationManager.Domain.Settings;

public static class RestaurantSettings
{
    public static TimeSpan OpenTime => new(12, 0, 0);

    public static TimeSpan CloseTime => new(22, 0, 0);

    public static TimeSpan MinBookingDuration => TimeSpan.FromHours(1.5);

    public static TimeSpan MaxBookingDuration => TimeSpan.FromHours(3.0);

    public static TimeSpan SlotInterval => TimeSpan.FromMinutes(30);
}
