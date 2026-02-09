using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Domain.Entities;

namespace ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IReservationRepository _reservationRepository;

    public CreateReservationCommandHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var reservationStart = request.ReservationDate;
        var reservationEnd = reservationStart.AddHours(request.DurationHours);

        var isOverlapping = await _reservationRepository.IsOverlapAsync(
            request.TableId,
            reservationStart,
            reservationEnd);

        if (isOverlapping)
        {
            throw new InvalidOperationException("Table is already booked for the requested time slot.");
        }

        var reservation = new Reservation(
            id: Guid.NewGuid(),
            tableId: request.TableId,
            customerName: request.CustomerName,
            customerEmail: request.CustomerEmail,
            customerPhone: request.CustomerPhone,
            reservationDate: request.ReservationDate,
            durationHours: (float)request.DurationHours);

        var reservationId = await _reservationRepository.AddAsync(reservation);

        return reservationId;
    }
}
