using MediatR;
using ReservationManager.Application.DTOs;

namespace ReservationManager.Application.Features.Reservations.Queries.GetReservationById;

public record GetReservationByIdQuery(Guid Id) : IRequest<ReservationDto>;
