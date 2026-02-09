using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IReservationRepository _repository;

    public CreateReservationCommandHandler(IReservationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var endTime = request.ReservationDate.AddHours(request.DurationHours);

        var isOverlapping = await _repository.IsOverlapAsync(
            request.TableId,
            request.ReservationDate,
            endTime);

        if (isOverlapping)
            throw new InvalidOperationException("Table is already booked for the selected time slot.");

        var reservation = new Reservation(
            Guid.NewGuid(),
            request.TableId,
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerPhone,
            request.ReservationDate,
            request.DurationHours);

        var reservationId = await _repository.AddAsync(reservation);

        return reservationId;
    }
}
