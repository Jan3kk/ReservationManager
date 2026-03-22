using FluentValidation;
using ReservationManager.Domain.Settings;

namespace ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.Date)
            .Must(d => d.Date >= DateTime.UtcNow.Date)
            .WithMessage("Date must be today or in the future.");

        RuleFor(x => x)
            .Must(BeAtLeastMinAdvanceBookingTime)
            .WithMessage($"Reservation must be made at least {RestaurantSettings.MinAdvanceBookingTime.TotalHours} hours in advance.");

        RuleFor(x => x.StartTime)
            .GreaterThanOrEqualTo(RestaurantSettings.OpenTime)
            .WithMessage($"Start time must be at or after {RestaurantSettings.OpenTime}.")
            .LessThan(RestaurantSettings.CloseTime)
            .WithMessage($"Start time must be before {RestaurantSettings.CloseTime}.");

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

        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .WithMessage("Customer name is required.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Customer email is required.")
            .EmailAddress()
            .WithMessage("Customer email must be a valid email address.");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .WithMessage("Customer phone is required.");
    }

    private static bool BeAtLeastMinAdvanceBookingTime(CreateReservationCommand command)
    {
        var reservationDateTime = DateTime.SpecifyKind(
            command.Date.Date + command.StartTime,
            DateTimeKind.Utc);
        var minAllowedTime = DateTime.UtcNow + RestaurantSettings.MinAdvanceBookingTime;

        return reservationDateTime >= minAllowedTime;
    }

    private static bool BeInValidIncrement(float durationHours)
    {
        var durationMinutes = (double)durationHours * 60.0;
        var incrementMinutes = RestaurantSettings.DurationIncrement.TotalMinutes;
        var remainder = durationMinutes % incrementMinutes;

        return remainder < 1e-6 || Math.Abs(remainder - incrementMinutes) < 1e-6;
    }
}
