using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Application.Payments.DTOs;

public sealed record CreatePaymentRequest(
    Guid BookingId,
    PaymentProvider Provider);
