
using ConcertTicket.Application.Payments.DTOs;
using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Application.Payments.Interfaces;

public interface IPaymentService
{
    Task<CreatePaymentResponse> CreateAsync(
        Guid userId,
        CreatePaymentRequest request,
        string ipAddress,
        CancellationToken cancellationToken);

    Task HandleCallbackAsync(
        PaymentProvider provider,
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken);

    Task<PaymentReturnResponse> HandleReturnAsync(
        PaymentProvider provider,
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken);
}