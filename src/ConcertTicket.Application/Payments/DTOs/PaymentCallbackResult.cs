namespace ConcertTicket.Application.Payments.DTOs;

public sealed record PaymentCallbackResult(
    bool IsValid,
    bool IsSuccess,
    string? TransactionReference,
    string? ResponsePayload);
