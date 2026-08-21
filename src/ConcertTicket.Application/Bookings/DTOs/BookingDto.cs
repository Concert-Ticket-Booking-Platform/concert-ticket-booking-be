namespace ConcertTicket.Application.Bookings.DTOs;

public sealed record BookingDto(
    Guid Id,
    string BookingCode,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    string Status,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt,
    Guid ConcertId,
    string ConcertName,
    IReadOnlyList<BookingItemDto> Items);

public sealed record BookingItemDto(
    Guid Id,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal,
    Guid TicketCategoryId,
    string TicketCategoryName);
