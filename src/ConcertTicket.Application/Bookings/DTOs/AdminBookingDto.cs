namespace ConcertTicket.Application.Bookings.DTOs;

public sealed record AdminBookingDto(
    Guid Id,
    string BookingCode,
    Guid UserId,
    string Username,
    Guid ConcertId,
    string ConcertName,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    string Status,
    int TotalTicketQuantity,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? CompletedAt
);