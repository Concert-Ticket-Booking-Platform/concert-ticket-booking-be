namespace ConcertTicket.Application.Payments.DTOs;

public sealed record PaymentReturnResponse(
    bool Success,
    string Message,
    string OrderCode,
    string? BookingCode,
    decimal Amount,
    string PaymentMethod,
    string? TransactionReference);
