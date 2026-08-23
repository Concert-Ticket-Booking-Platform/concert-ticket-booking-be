using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Application.Bookings.DTOs;

public sealed record CreateBookingResponse(
    Guid BookingId,
    string BookingCode,
    string Status,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    DateTimeOffset? ExpiresAt);