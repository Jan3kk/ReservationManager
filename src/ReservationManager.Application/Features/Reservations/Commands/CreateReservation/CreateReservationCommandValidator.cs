using FluentValidation;

namespace ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.TableId)
            .NotEmpty()
            .WithMessage("Table Id is required.");

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

        RuleFor(x => x.ReservationDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Reservation date must be in the future.");

        RuleFor(x => x.DurationHours)
            .InclusiveBetween(1, 3)
            .WithMessage("Duration must be between 1 and 3 hours.");
    }
}
