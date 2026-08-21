using ConcertTicket.Application.Bookings.DTOs;
using ConcertTicket.Application.Bookings.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTicket.Api.Controllers;

[ApiController]
[Route("api/v1/bookings")]
[Authorize]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(
        IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "Idempotency-Key")]
        string? idempotencyKey,
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest(new
            {
                message = "Idempotency-Key header is required."
            });
        }

        // Temporary user for local development.
        // Replace with authenticated user later.
        var userId = GetCurrentUserId();

        var result = await _bookingService.CreateAsync(
            userId,
            idempotencyKey,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.BookingId },
            result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var result = await _bookingService.GetByIdAsync(
            id,
            userId,
            cancellationToken);

        if (result is null)
        {
            return NotFound(new ConcertTicket.Api.Models.ApiResponse<object>
            {
                Success = false,
                Message = "Booking not found.",
                Errors = new List<string> { "Booking not found." }
            });
        }

        return Ok(new ConcertTicket.Api.Models.ApiResponse<BookingDto>
        {
            Success = true,
            Data = result
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var result = await _bookingService.GetAllBookingsAsync(
            userId,
            cancellationToken);

        return Ok(new ConcertTicket.Api.Models.ApiResponse<IReadOnlyList<BookingDto>>
        {
            Success = true,
            Data = result
        });
    }

    private Guid GetCurrentUserId()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var result))
        {
            throw new UnauthorizedAccessException(
                "Invalid user identity.");
        }

        return result;
    }
}
