using ConcertTicket.Application.TicketCategories.DTOs;
using ConcertTicket.Application.TicketCategories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTicket.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminTicketCategoriesController : ControllerBase
{
    private readonly IAdminTicketCategoryService _service;

    public AdminTicketCategoriesController(IAdminTicketCategoryService service)
    {
        _service = service;
    }

    [HttpPost("concerts/{concertId:guid}/ticket-categories")]
    public async Task<ActionResult<AdminTicketCategoryDto>> Create(
        Guid concertId,
        [FromBody] CreateTicketCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(
            concertId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet("concerts/{concertId:guid}/ticket-categories")]
    public async Task<ActionResult<IReadOnlyList<AdminTicketCategoryDto>>>
        GetByConcertId(
            Guid concertId,
            CancellationToken cancellationToken)
    {
        var result = await _service.GetByConcertIdAsync(
            concertId,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("ticket-categories/{id:guid}")]
    public async Task<ActionResult<AdminTicketCategoryDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(
            id,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("ticket-categories/{id:guid}")]
    public async Task<ActionResult<AdminTicketCategoryDto>> Update(
        Guid id,
        [FromBody] UpdateTicketCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("ticket-categories/{id:guid}/status")]
    public async Task<ActionResult<AdminTicketCategoryDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateTicketCategoryStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateStatusAsync(
            id,
            request,
            cancellationToken);

        return Ok(result);
    }
}
