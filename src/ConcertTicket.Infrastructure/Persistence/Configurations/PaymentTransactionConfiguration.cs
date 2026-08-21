using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertTicket.Infrastructure.Persistence.Configurations
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("payment_transactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasPrecision(12, 2);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.OrderCode)
                .HasColumnName("order_code");

            builder.Property(x => x.Provider)
                .HasColumnName("provider")
                .HasMaxLength(100);

            builder.Property(x => x.PaidAt)
                .HasColumnName("paid_at");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.HasOne(x => x.Booking)
                .WithMany(x => x.PaymentTransactions)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.BookingId);
            builder.HasIndex(x => x.OrderCode)
                .IsUnique();
        }
    }
}
