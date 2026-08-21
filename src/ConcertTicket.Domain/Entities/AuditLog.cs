namespace ConcertTicket.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public string ActorType { get; set; } = null!;

        public string Action { get; set; } = null!;

        public string ResourceType { get; set; } = null!;

        public Guid ResourceId { get; set; }

        public string? Metadata { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public Guid ActorId { get; set; }
        public User Actor { get; set; } = null!;
    }
}
