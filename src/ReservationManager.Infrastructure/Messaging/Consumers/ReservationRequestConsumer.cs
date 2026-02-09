using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

namespace ReservationManager.Infrastructure.Messaging.Consumers;

public class ReservationRequestConsumer : IConsumer<CreateReservationCommand>
{
    private readonly ISender _sender;
    private readonly ILogger<ReservationRequestConsumer> _logger;

    public ReservationRequestConsumer(ISender sender, ILogger<ReservationRequestConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateReservationCommand> context)
    {
        _logger.LogInformation(
            "Received reservation request for Table {TableId} on {ReservationDate}",
            context.Message.TableId,
            context.Message.ReservationDate);

        var reservationId = await _sender.Send(context.Message, context.CancellationToken);

        _logger.LogInformation(
            "Reservation {ReservationId} processed successfully",
            reservationId);
    }
}
