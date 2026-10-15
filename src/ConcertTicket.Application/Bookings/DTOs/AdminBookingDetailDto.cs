namespace ConcertTicket.Application.Bookings.DTOs;

public sealed record AdminBookingDetailDto(
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
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<AdminBookingItemDto> Items,
    IReadOnlyList<AdminBookingPaymentDto> Payments
);

public sealed record AdminBookingItemDto(
    Guid Id,
    Guid TicketCategoryId,
    string TicketCategoryName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal,
    DateTimeOffset CreatedAt
);

public sealed record AdminBookingPaymentDto(
    Guid Id,
    decimal Amount,
    string Status,
    long OrderCode,
    string Provider,
    string? TransactionReference,
    DateTimeOffset? PaidAt,
    DateTimeOffset? ExpiredAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
