using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReservationManager.Application.DTOs;
using ReservationManager.Application.Features.Tables.Commands.CreateTable;
using ReservationManager.Application.Features.Tables.Commands.DeleteTable;
using ReservationManager.Application.Features.Tables.Queries.GetAllTables;
using ReservationManager.Application.Features.Tables.Queries.GetTableById;

namespace ReservationManager.Api.Controllers;

[ApiController]
[Route("api/tables")]
public class TablesController : ControllerBase
{
    private readonly ISender _sender;

    public TablesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTableCommand command)
    {
        var tableId = await _sender.Send(command);

        return CreatedAtAction(nameof(GetById), new { id = tableId }, tableId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var table = await _sender.Send(new GetTableByIdQuery(id));

        return Ok(table);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TableDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tables = await _sender.Send(new GetAllTablesQuery());

        return Ok(tables);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteTableCommand(id));

        return NoContent();
    }
}
