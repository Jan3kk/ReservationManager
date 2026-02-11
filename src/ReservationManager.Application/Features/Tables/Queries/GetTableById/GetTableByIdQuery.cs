using MediatR;
using ReservationManager.Application.DTOs;

namespace ReservationManager.Application.Features.Tables.Queries.GetTableById;

public record GetTableByIdQuery(Guid Id) : IRequest<TableDto>;
