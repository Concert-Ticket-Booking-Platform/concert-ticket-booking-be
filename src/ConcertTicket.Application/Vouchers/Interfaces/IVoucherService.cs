using ConcertTicket.Domain.Entities;

namespace ConcertTicket.Application.Vouchers.Interfaces;

public interface IVoucherService
{
    Task<Voucher?> GetValidVoucherAsync(
        string code,
        CancellationToken cancellationToken);

    decimal CalculateDiscount(
        Voucher voucher,
        decimal subtotal);
}
