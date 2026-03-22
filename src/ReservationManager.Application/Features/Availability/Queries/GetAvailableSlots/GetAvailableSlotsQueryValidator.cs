using FluentValidation;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Application.Features.Availability.Queries.GetAvailableSlots;

public class GetAvailableSlotsQueryValidator : AbstractValidator<GetAvailableSlotsQuery>
{
    public GetAvailableSlotsQueryValidator()
    {
        RuleFor(x => x.Date)
            .Must(d => d.Date >= DateTime.UtcNow.Date)
            .WithMessage("Date must be today or in the future.");

        RuleFor(x => x.PartySize)
            .InclusiveBetween(RestaurantSettings.MinPartySize, RestaurantSettings.MaxPartySize)
            .WithMessage($"Party size must be between {RestaurantSettings.MinPartySize} and {RestaurantSettings.MaxPartySize}.");

        RuleFor(x => x.DurationHours)
            .GreaterThanOrEqualTo((float)RestaurantSettings.MinBookingDuration.TotalHours)
            .WithMessage($"Duration must be at least {RestaurantSettings.MinBookingDuration.TotalHours} hours.")
            .LessThanOrEqualTo((float)RestaurantSettings.MaxBookingDuration.TotalHours)
            .WithMessage($"Duration must not exceed {RestaurantSettings.MaxBookingDuration.TotalHours} hours.")
            .Must(BeInValidIncrement)
            .WithMessage($"Duration must be in {RestaurantSettings.DurationIncrement.TotalMinutes}-minute increments (e.g., 1.5, 2.0, 2.5, 3.0 hours).");
    }

    private static bool BeInValidIncrement(float durationHours)
    {
        var durationMinutes = (double)durationHours * 60.0;
        var incrementMinutes = RestaurantSettings.DurationIncrement.TotalMinutes;
        var remainder = durationMinutes % incrementMinutes;

        return remainder < 1e-6 || Math.Abs(remainder - incrementMinutes) < 1e-6;
    }
}
