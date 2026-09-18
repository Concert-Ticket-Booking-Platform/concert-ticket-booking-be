using ConcertTicket.Application.Concerts.DTOs;
using ConcertTicket.Application.Concerts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConcertTicket.Api.Controllers;

[ApiController]
[Route("api/v1/admin/concerts")]
[Authorize(Roles = "Admin")]
public sealed class AdminConcertsController : ControllerBase
{
    private readonly IAdminConcertService _concertService;

    public AdminConcertsController(IAdminConcertService concertService)
    {
        _concertService = concertService;
    }

    [HttpPost]
    public async Task<ActionResult<AdminConcertDto>> Create(
        [FromBody] CreateConcertRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        var result = await _concertService.CreateAsync(
            request,
            currentUserId,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { concertId = result.Id },
            result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminConcertDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _concertService.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{concertId:guid}")]
    public async Task<ActionResult<AdminConcertDto>> GetById(
        Guid concertId,
        CancellationToken cancellationToken)
    {
        var result = await _concertService.GetByIdAsync(
            concertId,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("{concertId:guid}")]
    public async Task<ActionResult<AdminConcertDto>> Update(
        Guid concertId,
        [FromBody] UpdateConcertRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _concertService.UpdateAsync(
            concertId,
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{concertId:guid}/status")]
    public async Task<ActionResult<AdminConcertDto>> UpdateStatus(
        Guid concertId,
        [FromBody] UpdateConcertStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _concertService.UpdateStatusAsync(
            concertId,
            request,
            cancellationToken);

        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var currentUserId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user identity.");
        }

        return currentUserId;
    }
}
