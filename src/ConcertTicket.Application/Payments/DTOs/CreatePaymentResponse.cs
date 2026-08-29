namespace ConcertTicket.Application.Payments.DTOs;

public sealed record CreatePaymentResponse(
    Guid PaymentId,
    Guid BookingId,
    string Provider,
    decimal Amount,
    string PaymentUrl,
    DateTimeOffset? ExpiresAt);
