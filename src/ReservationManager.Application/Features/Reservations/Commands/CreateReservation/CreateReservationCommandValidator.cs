using FluentValidation;

namespace ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.TableId)
            .NotEmpty()
            .WithMessage("Table ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .WithMessage("Customer name is required.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Customer email is required.")
            .EmailAddress()
            .WithMessage("Invalid email address format.");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .WithMessage("Customer phone is required.");

        RuleFor(x => x.ReservationDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Reservation date must be in the future.");

        RuleFor(x => x.DurationHours)
            .InclusiveBetween(1f, 3f)
            .WithMessage("Duration must be between 1 and 3 hours.");

        RuleFor(x => x.DurationHours)
            .Must(BeValidIncrement)
            .WithMessage("Duration must be in 0.5 hour increments.");
    }

    private bool BeValidIncrement(float duration)
    {
        return (duration * 2) % 1 == 0;
    }
}
