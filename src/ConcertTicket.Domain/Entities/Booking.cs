using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }

        public string BookingCode { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal FinalAmount { get; set; }

        public BookingStatus Status { get; set; }

        public DateTimeOffset? ExpiresAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public string IdempotencyKey { get; set; } = null!;

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public Guid ConcertId { get; set; }

        public Concert Concert { get; set; } = null!;

        public ICollection<BookingItem> BookingItems { get; set; }
            = new List<BookingItem>();

        public ICollection<PaymentTransaction> PaymentTransactions { get; set; }
            = new List<PaymentTransaction>();

        public ICollection<VoucherUsage> VoucherUsages { get; set; }
            = new List<VoucherUsage>();
    }
}
