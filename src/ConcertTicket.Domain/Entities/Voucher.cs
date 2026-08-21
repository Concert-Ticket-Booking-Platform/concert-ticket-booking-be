using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Domain.Entities
{
    public class Voucher
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = null!;

        public string Name { get; set; } = null!;

        public DiscountType DiscountType { get; set; }

        public decimal DiscountValue { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        public int UsageLimit { get; set; }

        public int UsedCount { get; set; }

        public DateTimeOffset StartsAt { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }

        public VoucherStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public ICollection<VoucherUsage> Usages { get; set; }
            = new List<VoucherUsage>();
    }
}
