using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertTicket.Infrastructure.Persistence.Configurations
{
    public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
    {
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            builder.ToTable("vouchers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.Code)
                .IsUnique();

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.DiscountType)
                .HasColumnName("discount_type")
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.DiscountValue)
                .HasColumnName("discount_value")
                .HasPrecision(12, 2);

            builder.Property(x => x.MaxDiscountAmount)
                .HasColumnName("max_discount_amount")
                .HasPrecision(12, 2);

            builder.Property(x => x.UsageLimit)
                .HasColumnName("usage_limit");

            builder.Property(x => x.UsedCount)
                .HasColumnName("used_count");

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.StartsAt)
                .HasColumnName("starts_at");

            builder.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }
}
