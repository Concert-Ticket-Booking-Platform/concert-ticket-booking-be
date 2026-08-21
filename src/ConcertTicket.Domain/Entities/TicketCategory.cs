using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Domain.Entities
{
    public class TicketCategory
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public decimal Price { get; set; }

        public int TotalQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        public int ReservedQuantity { get; set; }

        public int SoldQuantity { get; set; }

        public TicketCategoryStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public Guid ConcertId { get; set; }

        public Concert Concert { get; set; } = null!;

        public ICollection<BookingItem> BookingItems { get; set; }
            = new List<BookingItem>();
    }
}
