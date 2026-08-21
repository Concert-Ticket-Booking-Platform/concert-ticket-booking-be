namespace ConcertTicket.Domain.Entities
{
    public class BookingItem
    {
        public Guid Id { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Subtotal { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public Guid TicketCategoryId { get; set; }

        public TicketCategory TicketCategory { get; set; } = null!;

        public Guid BookingId { get; set; }

        public Booking Booking { get; set; } = null!;
    }
}
