using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Domain.Entities
{
    public class PaymentTransaction
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; }

        public long OrderCode { get; set; }

        public PaymentProvider Provider { get; set; }

        public string? TransactionReference { get; set; }

        public string? ResponsePayload { get; set; }

        public DateTimeOffset? PaidAt { get; set; }

        public DateTimeOffset? ExpiredAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public Guid BookingId { get; set; }

        public Booking Booking { get; set; } = null!;
    }
}
