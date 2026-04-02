using FluentValidation;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Application.Features.Availability.Queries.GetAvailableSlots;

public class GetAvailableSlotsQueryValidator : AbstractValidator<GetAvailableSlotsQuery>
{
    public GetAvailableSlotsQueryValidator()
    {
        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
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
            .WithMessage($"Duration must be in {RestaurantSettings.DurationIncrement.TotalMinutes}-minute increments.");
    }

    private static bool BeInValidIncrement(float durationHours)
    {
        var durationMinutes = durationHours * 60;
        var incrementMinutes = RestaurantSettings.DurationIncrement.TotalMinutes;

        return durationMinutes % incrementMinutes == 0;
    }
}
