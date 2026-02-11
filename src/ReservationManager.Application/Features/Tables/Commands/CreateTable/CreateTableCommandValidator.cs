using FluentValidation;

namespace ReservationManager.Application.Features.Tables.Commands.CreateTable;

public class CreateTableCommandValidator : AbstractValidator<CreateTableCommand>
{
    public CreateTableCommandValidator()
    {
        RuleFor(x => x.UniqueName)
            .NotEmpty()
            .WithMessage("UniqueName is required.");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Label is required.");

        RuleFor(x => x.Capacity)
            .InclusiveBetween(2, 10)
            .WithMessage("Capacity must be between 2 and 10.");
    }
}
