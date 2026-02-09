using MediatR;

namespace ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(
    Guid TableId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    DateTime ReservationDate,
    double DurationHours) : IRequest<Guid>;
