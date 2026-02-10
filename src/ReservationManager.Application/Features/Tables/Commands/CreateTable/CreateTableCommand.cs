using MediatR;

namespace ReservationManager.Application.Features.Tables.Commands.CreateTable;

public record CreateTableCommand(string Name, int Capacity) : IRequest<Guid>;
