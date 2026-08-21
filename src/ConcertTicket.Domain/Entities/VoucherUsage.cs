namespace ConcertTicket.Domain.Entities
{
    public class VoucherUsage
    {
        public Guid Id { get; set; }

        public DateTimeOffset UsedAt { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public Guid BookingId { get; set; }

        public Booking Booking { get; set; } = null!;

        public Guid VoucherId { get; set; }

        public Voucher Voucher { get; set; } = null!;
    }
}
