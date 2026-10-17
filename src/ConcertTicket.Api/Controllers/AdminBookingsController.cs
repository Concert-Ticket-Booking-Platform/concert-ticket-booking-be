using ConcertTicket.Application.Bookings.DTOs;
using ConcertTicket.Application.Bookings.Interfaces;
using ConcertTicket.Application.Common.Models;
using ConcertTicket.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTicket.Api.Controllers;

public sealed class AdminBookingsController : ControllerBase
{
    private readonly IAdminBookingService _bookingService;

    public AdminBookingsController(
        IAdminBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminBookingDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] BookingStatus? status = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? concertId = null,
        [FromQuery] string? bookingCode = null,
        [FromQuery] DateTimeOffset? fromDate = null,
        [FromQuery] DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookingService.GetAllAsync(
            page: page,
            pageSize: pageSize,
            status: status,
            userId: userId,
            concertId: concertId,
            bookingCode: bookingCode,
            fromDate: fromDate,
            toDate: toDate,
            cancellationToken: cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminBookingDetailDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(result);
    }
}
