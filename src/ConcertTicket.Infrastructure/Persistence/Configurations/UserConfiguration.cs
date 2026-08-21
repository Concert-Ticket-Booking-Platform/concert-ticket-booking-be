using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertTicket.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Username)
                .HasColumnName("username")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Password)
                .HasColumnName("password")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Phone)
                .HasColumnName("phone")
                .HasMaxLength(30);

            builder.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100);

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.HasIndex(x => x.Username);

            builder.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
