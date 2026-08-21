using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertTicket.Infrastructure.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("audit_logs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ActorId)
                .HasColumnName("actor_id")
                .IsRequired();

            builder.Property(x => x.ActorType)
                .HasColumnName("actor_type")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Action)
                .HasColumnName("action")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ResourceType)
                .HasColumnName("resource_type")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ResourceId)
                .HasColumnName("resource_id")
                .IsRequired();

            builder.Property(x => x.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            // Audit log belongs to the user who performed the action.
            // Restrict deletion to preserve audit history.
            builder.HasOne(x => x.Actor)
                .WithMany()
                .HasForeignKey(x => x.ActorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ActorId);

            builder.HasIndex(x => new
            {
                x.ResourceType,
                x.ResourceId
            });

            builder.HasIndex(x => x.CreatedAt);
        }
    }
}
