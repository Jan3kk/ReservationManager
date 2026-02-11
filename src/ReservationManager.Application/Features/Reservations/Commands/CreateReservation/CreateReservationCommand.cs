using MediatR;

namespace ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(
    DateTime Date,
    TimeSpan StartTime,
    float DurationHours,
    int PartySize,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone) : IRequest<Guid>;
