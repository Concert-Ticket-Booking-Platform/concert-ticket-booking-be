
using ConcertTicket.Application.Payments.DTOs;
using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Application.Common.Interfaces;

public interface IPaymentProvider
{
    PaymentProvider Provider { get; }

    Task<PaymentCreationResult> CreatePaymentAsync(
        PaymentCreationRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentCallbackResult> ProcessCallbackAsync(
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken = default);
}

public sealed record PaymentCreationRequest(
    Guid PaymentId,
    long OrderCode,
    decimal Amount,
    string Description,
    string ReturnUrl,
    string IpAddress,
    DateTimeOffset ExpiresAt);

public sealed record PaymentCreationResult(
    bool Success,
    string PaymentUrl,
    string? ErrorMessage);