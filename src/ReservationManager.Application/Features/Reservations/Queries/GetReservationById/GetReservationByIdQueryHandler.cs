using MediatR;
using ReservationManager.Application.Abstractions.Repositories;
using ReservationManager.Application.DTOs;

namespace ReservationManager.Application.Features.Reservations.Queries.GetReservationById;

public class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, ReservationDto?>
{
    private readonly IReservationRepository _reservationRepository;

    public GetReservationByIdQueryHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<ReservationDto?> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(request.Id);

        if (reservation is null)
            return null;

        return new ReservationDto(
            reservation.Id,
            reservation.TableId,
            reservation.CustomerName,
            reservation.CustomerEmail,
            reservation.CustomerPhone,
            reservation.ReservationDate,
            reservation.DurationHours,
            reservation.PartySize,
            reservation.Status);
    }
}
