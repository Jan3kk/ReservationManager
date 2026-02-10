using MediatR;

namespace ReservationManager.Application.Features.Tables.Commands.DeleteTable;

public record DeleteTableCommand(Guid Id) : IRequest;
