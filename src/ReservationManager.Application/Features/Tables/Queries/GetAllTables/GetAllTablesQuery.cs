using MediatR;
using ReservationManager.Application.DTOs;

namespace ReservationManager.Application.Features.Tables.Queries.GetAllTables;

public record GetAllTablesQuery : IRequest<List<TableDto>>;
