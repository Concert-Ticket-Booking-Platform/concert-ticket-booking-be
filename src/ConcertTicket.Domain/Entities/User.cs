using System.Data;

namespace ConcertTicket.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? Phone { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public Guid RoleId { get; set; }

        public Role Role { get; set; } = null!;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
    }
}
