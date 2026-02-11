using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReservationManager.Application.DTOs;
using ReservationManager.Application.Features.Availability.Queries.GetAvailableSlots;

namespace ReservationManager.Api.Controllers;

[ApiController]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly ISender _sender;

    public AvailabilityController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TimeSlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] DateTime date,
        [FromQuery] int partySize,
        [FromQuery] float duration)
    {
        var query = new GetAvailableSlotsQuery(date, partySize, duration);
        var result = await _sender.Send(query);

        return Ok(result);
    }
}
