using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Domain.Entities
{
    public class Concert
    {
        public Guid Id { get; set; }

        public string ConcertName { get; set; } = null!;

        public string? Description { get; set; }

        public string Venue { get; set; } = null!;

        public DateTimeOffset EventDate { get; set; }

        public ConcertStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        public User Creator { get; set; }

        public ICollection<TicketCategory> TicketCategories { get; set; }
            = new List<TicketCategory>();

        public ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();
    }
}
