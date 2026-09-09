namespace ConcertTicket.Application.Payments.DTOs;

public sealed record MoMoCreatePaymentRequest(
    string PartnerCode,
    string RequestType,
    string IpnUrl,
    string RedirectUrl,
    string OrderId,
    long Amount,
    string OrderInfo,
    string RequestId,
    string ExtraData,
    string Lang,
    string Signature);
