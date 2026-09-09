namespace ConcertTicket.Application.Payments.DTOs;

public sealed record MoMoCreatePaymentResponse(
    string PartnerCode,
    string RequestId,
    string OrderId,
    long Amount,
    long ResponseTime,
    string Message,
    int ResultCode,
    string? PayUrl,
    string? Deeplink,
    string? QrCodeUrl,
    string? Signature);
