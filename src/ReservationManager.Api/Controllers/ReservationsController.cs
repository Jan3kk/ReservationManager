using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReservationManager.Application.Features.Reservations.Commands.CreateReservation;

namespace ReservationManager.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly ISender _sender;

    public ReservationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReservationCommand command)
    {
        var reservationId = await _sender.Send(command);

        return CreatedAtAction(nameof(GetById), new { id = reservationId }, reservationId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // TODO: Implement GetReservationByIdQuery
        return Ok(id);
    }
}
