using ConcertTicket.Domain.Entities;
using ConcertTicket.Application.Vouchers.DTOs;

namespace ConcertTicket.Application.Vouchers.Interfaces;

public interface IVoucherService
{
    Task<IReadOnlyList<VoucherDto>> GetAllVouchersAsync(
        CancellationToken cancellationToken);

    Task<Voucher?> GetValidVoucherAsync(
        string code,
        CancellationToken cancellationToken);

    decimal CalculateDiscount(
        Voucher voucher,
        decimal subtotal);
}
