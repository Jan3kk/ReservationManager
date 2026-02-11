using MediatR;

namespace ReservationManager.Application.Features.Tables.Commands.CreateTable;

public record CreateTableCommand(string UniqueName, string Label, int Capacity) : IRequest<Guid>;
