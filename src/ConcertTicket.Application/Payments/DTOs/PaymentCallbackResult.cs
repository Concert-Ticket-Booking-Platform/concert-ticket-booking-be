namespace ConcertTicket.Application.Payments.DTOs;

public sealed record PaymentCallbackResult(
    bool IsValid,
    bool IsSuccess,
    long? Amount,
    string? TransactionReference,
    string? ResponsePayload);
